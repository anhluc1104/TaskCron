Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions

Public Module cron_get_retela_quote_product_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()

        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Dim targetCount = 0
        Dim executeCount = 0
        Try
            Dim responce = RetelaAPI.GetQuoteProductMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of QuoteProductResponce)(responce)
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    'log.LogPut(CommonConst.LogLever_Notice, "GetQuoteProductMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetQuoteProductMasterData")
                    PageDataSet.M_QuoteProduct.Clear()
                    Using ta As New dsCronTableAdapters.M_QuoteProductTableAdapter(connectionString)
                        Dim idCode = If(ta.MaxIDQuery Is Nothing, 0, ta.MaxIDQuery) + 1
                        For Each item In list


                            Using taCateogryL As New dsCronTableAdapters.M_CategoryLTableAdapter(connectionString)

                                If Not taCateogryL.GetDataByRetelaDaiCd().Any(Function(x) x.retela_dai_code = item.DaiCd) Then
                                    log.LogPut(CommonConst.LogLever_Error, "大分類未登録のためスキップ[" + item.DaiCd.ToString + "]", "GetQuoteProductMasterData")
                                    Continue For

                                End If


                            End Using

                            Using taShop As New dsCronTableAdapters.M_ShopTableAdapter(connectionString)

                                Dim shops As List(Of String) = item.TargetShop.Split(","c).ToList()
                                Dim shopretela = taShop.GetDataByDelFlg()
                                item.TargetShop = ""

                                For Each shopcd In shops

                                    If Not shopretela.Any(Function(x) x.retela_shop_code = shopcd) Then
                                        log.LogPut(CommonConst.LogLever_Error, "店舗未登録のためスキップ[" + shopcd.ToString + "]", "GetQuoteProductMasterData")
                                        Continue For
                                    Else
                                        item.TargetShop = JoinStringItems(item.TargetShop, shopretela.First(Function(x) x.retela_shop_code = shopcd).id_code, ",")
                                    End If
                                Next

                                If String.IsNullOrEmpty(item.TargetShop) Then
                                    Continue For
                                End If

                            End Using

                            If ta.FillByRetelaItemCode(PageDataSet.M_QuoteProduct, item.ItemCd) = 1 Then
                                With PageDataSet.M_QuoteProduct.FirstOrDefault
                                    .category_l_id_code = item.DaiCd
                                    .product_name = item.ItemName
                                    .product_name_pos = item.ItemNamePOS
                                    .tax_class = item.ZeiKbn
                                    .selling_unit_price = item.SalesPrice
                                    .target_shop = item.TargetShop
                                    .jan_code = item.JanCode
                                    .item_code = item.ItemCd
                                    .cost_price = item.CostPrice
                                    .update_date = currentTime
                                    .draft_flg = 0
                                    .del_flg = item.DelFlg
                                End With
                            Else
                                Dim dr = PageDataSet.M_QuoteProduct.NewM_QuoteProductRow
                                With dr
                                    .id_code = idCode
                                    .category_l_id_code = item.DaiCd
                                    .product_name = item.ItemName
                                    .product_name_pos = item.ItemNamePOS
                                    .tax_class = item.ZeiKbn
                                    .selling_unit_price = item.SalesPrice
                                    .target_shop = item.TargetShop
                                    .jan_code = item.JanCode
                                    .item_code = item.ItemCd
                                    .cost_price = item.CostPrice
                                    .sort_num = idCode
                                    .draft_flg = 0
                                    .del_flg = item.DelFlg
                                    .update_date = currentTime
                                    .update_user = ""
                                End With
                                PageDataSet.M_QuoteProduct.AddM_QuoteProductRow(dr)
                                idCode += 1
                            End If
                            executeCount += 1
                        Next
                    End Using


                    If DataUpdate(connectionString) Then
                    End If


                End If

                'Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAから商品マスタの差分取り込みが完了しました。"
                'log.LogPut(CommonConst.LogLever_Notice, "cron_GetQuoteProductMasterData " & logMessage, "GetQuoteProductMasterData")
                'log.LogPut(CommonConst.LogLever_Notice, "cron_GetQuoteProductMasterData succesfully!", "GetQuoteProductMasterData")

            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetQuoteProductMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetQuoteProductMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetQuoteProductMasterData")
        End Try

    End Sub

    Private Function DataUpdate(connectionString As String) As Boolean

        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim sqlCommand = ""
        Dim updateFlg = True
        Try

            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_QuoteProductTableAdapter(connectionString)
                    'sqlCommand = ta.GetUpdateCommandText()
                    'log.LogPut(CommonConst.LogLever_Notice, sqlCommand, "GetQuoteProductMasterData")
                    If ta.Update(PageDataSet.M_QuoteProduct) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                'For Each row In PageDataSet.M_QuoteProduct
                '    Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                '        + "], category_l_id_code: [" + row("category_l_id_code").ToString() _
                '        + "], product_name: [" + row("product_name").ToString() _
                '        + "], sort_num: [" + row("sort_num").ToString() _
                '        + "], draft_flg: [" + row("draft_flg").ToString() _
                '        + "], update_user: [" + row("update_user").ToString() _
                '        + "], update_date: [" + row("update_date").ToString() _
                '        + "], del_flg: [" + row("del_flg").ToString()
                '    log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetQuoteProductMasterData")
                '    log.LogPut(CommonConst.LogLever_Notice, "Executed sql successful!", "GetQuoteProductMasterData")
                'Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetQuoteProductMasterData")
            Return False
        End Try
        Return True

    End Function

    Private Function JoinStringItems(items As String, itemNew As String, separator As String) As String

        Dim splitItems As List(Of String) = items.Split(New String() {separator}, StringSplitOptions.RemoveEmptyEntries).ToList()
        If Not splitItems.Contains(itemNew) Then
            splitItems.Add(itemNew)
        End If
        Return String.Join(separator, splitItems)
    End Function
End Module
Public Class QuoteProductResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of QuoteProductMaster)
End Class

Public Class QuoteProductMaster
    Public Property DaiCd As Integer
    Public Property ItemName As String
    Public Property ItemNamePOS As String
    Public Property ZeiKbn As Integer
    Public Property SalesPrice As Integer
    Public Property TargetShop As String
    Public Property JanCode As String
    Public Property ItemCd As String
    Public Property CostPrice As Integer
    Public Property DelFlg As String
End Class
