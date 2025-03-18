Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions

Public Module cron_get_retela_shop_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()

        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Dim targetCount = 0
        Dim executeCount = 0
        Try
            Dim responce = RetelaAPI.GetShopMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of ShopResponce)(responce)
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    log.LogPut(CommonConst.LogLever_Notice, "GetShopMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetShopMasterData")

                    For Each item In list
                        PageDataSet.M_Shop.Clear()

                        Using taFc As New dsCronTableAdapters.M_FranchiseTableAdapter(connectionString)

                            Dim dtFC = taFc.GetData()

                            If Not dtFC.Any(Function(x) x.retela_fc_code = item.FcCd) Then
                                log.LogPut(CommonConst.LogLever_Error, "フランチャイズ会社マスタに存在しないFCコードがリテラから渡されました。フランチャイズ会社マスタを更新後に再度実行して下さい。", "GetShopMasterData")
                                Continue For
                            End If

                            Using ta As New dsCronTableAdapters.M_ShopTableAdapter(connectionString)

                                ta.FillByRetelaShopCd(PageDataSet.M_Shop, item.ShopCd)

                                If PageDataSet.M_Shop.Any() Then

                                    With PageDataSet.M_Shop(0)
                                        .shop_code = item.ShopCd
                                        .shop_name = item.ShopName
                                        .fc_id_code = dtFC.FirstOrDefault(Function(x) x.retela_fc_code = item.FcCd).id_code
                                        .retela_shop_code = item.ShopCd
                                        .tel = item.Tel
                                        .email_address = item.Email
                                        .update_user = ""
                                        .update_date = currentTime
                                        .del_flg = item.DelFlg
                                    End With

                                Else
                                    Dim dr = PageDataSet.M_Shop.NewM_ShopRow
                                    Dim idCode = If(ta.MaxIdQuery Is Nothing, 0, ta.MaxIdQuery) + 1
                                    With dr
                                        .id_code = idCode
                                        .shop_code = item.ShopCd
                                        .shop_name = item.ShopName
                                        .fc_id_code = dtFC.FirstOrDefault(Function(x) x.retela_fc_code = item.FcCd).id_code
                                        .retela_shop_code = item.ShopCd
                                        .tel = item.Tel
                                        .email_address = item.Email
                                        .sort_num = idCode
                                        .draft_flg = 0
                                        .del_flg = item.DelFlg
                                        .update_date = currentTime
                                        .update_user = ""
                                    End With
                                    PageDataSet.M_Shop.AddM_ShopRow(dr)
                                End If
                            End Using

                            If DataUpdate(connectionString) Then
                                executeCount += 1
                            End If
                        End Using

                    Next

                End If
                Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAから店舗マスタの差分取り込みが完了しました。"
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetShopMasterData " & logMessage, "GetShopMasterData")
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetShopMasterData succesfully!", "GetShopMasterData")
            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetShopMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetShopMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetShopMasterData")
        End Try

    End Sub

    Private Function DataUpdate(connectionString As String) As Boolean

        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim sqlCommand = ""
        Dim updateFlg = True
        Try

            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_ShopTableAdapter(connectionString)
                    sqlCommand = ta.GetUpdateCommandText()
                    If ta.Update(PageDataSet.M_Shop) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                For Each row In PageDataSet.M_Shop
                    log.LogPut(CommonConst.LogLever_Notice, sqlCommand, "GetShopMasterData")
                    Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                        + "], shop_code: [" + row("shop_code").ToString() _
                        + "], shop_name: [" + row("shop_name").ToString() _
                        + "], fc_id_code: [" + row("fc_id_code").ToString() _
                        + "], sort_num: [" + row("sort_num").ToString() _
                        + "], draft_flg: [" + row("draft_flg").ToString() _
                        + "], update_user: [" + row("update_user").ToString() _
                        + "], update_date: [" + row("update_date").ToString() _
                        + "], del_flg: [" + row("del_flg").ToString()
                    log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetShopMasterData")
                    log.LogPut(CommonConst.LogLever_Notice, "Connection successful!", "GetShopMasterData")
                Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetShopMasterData")
            Return False
        End Try
        Return True

    End Function
End Module
Public Class ShopResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of ShopMaster)
End Class

Public Class ShopMaster
    Public Property ShopCd As String
    Public Property ShopName As String
    Public Property FcCd As String
    Public Property Tel As String
    Public Property Email As String
    Public Property DelFlg As Integer
End Class
