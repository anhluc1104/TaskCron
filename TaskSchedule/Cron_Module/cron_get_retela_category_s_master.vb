Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions

Public Module cron_get_retela_category_s_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Dim targetCount = 0
        Dim executeCount = 0
        Try
            Dim reponceStr As String = RetelaAPI.GetCatagorySMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of CatagorySResponce)(reponceStr)
            Dim result = ret.Result
            Dim message = ret.Message
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    log.LogPut(CommonConst.LogLever_Notice, "GetCategorySMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetCategorySMasterData")
                    PageDataSet.M_CategoryM.Clear()
                    Using ta As New dsCronTableAdapters.M_CategorySTableAdapter(connectionString)
                        Dim idCode = CInt(ta.MaxIDQuery) + 1
                        For Each item In list
                            If ta.FillByDaiCdAndCyuCdAndSyoCd(PageDataSet.M_CategoryS, item.DaiCd, item.CyuCd, item.CyuCd) > 0 Then
                                With PageDataSet.M_CategoryS.Last
                                    .update_date = currentTime
                                    .del_flg = item.delFlg
                                    .draft_flg = 0
                                    .category_name = item.SyoName
                                    .DaiCd = item.DaiCd
                                    .CyuCd = item.CyuCd
                                    .SyoCd = item.SyoCd
                                End With
                            Else
                                Dim dr = PageDataSet.M_CategoryS.NewM_CategorySRow
                                With dr
                                    .id_code = idCode
                                    .category_name = item.SyoName
                                    .DaiCd = item.DaiCd
                                    .CyuCd = item.CyuCd
                                    .SyoCd = item.SyoCd
                                    .sort_num = idCode
                                    .draft_flg = 0
                                    .del_flg = item.delFlg
                                    .update_date = currentTime
                                    .update_user = ""
                                    .comment = ""
                                End With
                                PageDataSet.M_CategoryS.AddM_CategorySRow(dr)
                                idCode += 1
                            End If
                            executeCount += 1
                        Next
                    End Using
                    If DataUpdate(connectionString) Then
                    End If
                End If
                Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAから小分類マスタの差分取り込みが完了しました。"
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategorySMasterData " & logMessage, "GetCategorySMasterData")
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategorySMasterData succesfully!", "GetCategorySMasterData")
            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategorySMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetCategorySMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetCategorySMasterData")
        End Try

    End Sub
    Private Function DataUpdate(connectionString As String) As Boolean
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim sqlCommand = ""
        Dim updateFlg = True
        Try
            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_CategorySTableAdapter(connectionString)
                    sqlCommand = ta.GetUpdateCommandText()
                    If ta.Update(PageDataSet.M_CategoryS) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                For Each row In PageDataSet.M_CategoryS
                    log.LogPut(CommonConst.LogLever_Notice, sqlCommand, "GetCategorySMasterData")
                    Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                        + "], DaiCd: [" + row("DaiCd").ToString() _
                        + "], CyuCd: [" + row("CyuCd").ToString() _
                        + "], SyoCd: [" + row("SyoCd").ToString() _
                        + "], category_name: [" + row("category_name").ToString() _
                        + "], comment: [" + row("comment").ToString() _
                        + "], sort_num: [" + row("sort_num").ToString() _
                        + "], draft_flg: [" + row("draft_flg").ToString() _
                        + "], update_user: [" + row("update_date").ToString() _
                        + "], update_date: [" + row("update_date").ToString() _
                        + "], del_flg: [" + row("del_flg").ToString() + "]"
                    log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetCategorySMasterData")
                    log.LogPut(CommonConst.LogLever_Notice, "Connection successful!", "GetCategorySMasterData")
                Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetCategorySMasterData")
            Return False
        End Try
        Return True
    End Function
End Module
Public Class CatagorySResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of CatagorySMaster)
End Class
Public Class CatagorySMaster
    Public Property DaiCd As Integer
    Public Property CyuCd As Integer
    Public Property SyoCd As Integer
    Public Property SyoName As String
    Public Property TurnNo As Integer
    Public Property delFlg As String
End Class
