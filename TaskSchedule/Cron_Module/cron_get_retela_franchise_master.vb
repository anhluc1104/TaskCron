Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions

Public Module cron_get_retela_franchise_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Dim targetCount = 0
        Dim executeCount = 0
        Try
            Dim responce = RetelaAPI.GetFranchiseMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of FranchiseResponce)(responce)
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    log.LogPut(CommonConst.LogLever_Notice, "GetFranchiseMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetFranchiseMasterData")
                    For Each item In list

                        PageDataSet.M_Franchise.Clear()
                        Using ta As New dsCronTableAdapters.M_FranchiseTableAdapter(connectionString)

                            ta.FillByRetelaFranchiseCd(PageDataSet.M_Franchise, item.FcCd)

                            If PageDataSet.M_Franchise.Any() Then

                                With PageDataSet.M_Franchise(0)
                                    .fc_name = item.FcName
                                    .retela_fc_code = item.FcCd
                                    .del_flg = item.DelFlg
                                    .update_user = ""
                                    .update_date = currentTime
                                End With

                            Else

                                ' Nếu không tìm thấy, thêm mới một hàng
                                Dim dr = PageDataSet.M_Franchise.NewM_FranchiseRow
                                Dim idCode = CInt(ta.MaxIdQuery) + 1

                                With dr
                                    .id_code = idCode
                                    .fc_name = item.FcName
                                    .retela_fc_code = item.FcCd
                                    .sort_num = idCode
                                    .draft_flg = 0
                                    .del_flg = item.DelFlg
                                    .update_date = currentTime
                                    .update_user = ""
                                End With

                                PageDataSet.M_Franchise.AddM_FranchiseRow(dr)
                            End If

                            If DataUpdate(connectionString) Then
                                executeCount += 1
                            End If

                        End Using

                    Next
                End If
                Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAからフランチャイズ会社マスタの差分取り込みが完了しました。"
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetFranchiseMasterData " & logMessage, "GetFranchiseMasterData")
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetFranchiseMasterData succesfully!", "GetFranchiseMasterData")
            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetFranchiseMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetFranchiseMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetFranchiseMasterData")
        End Try

    End Sub
    Private Function DataUpdate(connectionString As String) As Boolean

        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim sqlCommand = ""
        Dim updateFlg = True
        Try

            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_FranchiseTableAdapter(connectionString)
                    sqlCommand = ta.GetUpdateCommandText()
                    If ta.Update(PageDataSet.M_Franchise) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                For Each row In PageDataSet.M_Franchise
                    log.LogPut(CommonConst.LogLever_Notice, sqlCommand, "GetFranchiseMasterData")
                    Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                        + "], fc_name: [" + row("fc_name").ToString() _
                        + "], image1: [" + row("image1").ToString() _
                        + "], sort_num: [" + row("sort_num").ToString() _
                        + "], draft_flg: [" + row("draft_flg").ToString() _
                        + "], update_user: [" + row("update_user").ToString() _
                        + "], update_date: [" + row("update_date").ToString() _
                        + "], del_flg: [" + row("del_flg").ToString()
                    log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetFranchiseMasterData")
                    log.LogPut(CommonConst.LogLever_Notice, "Connection successful!", "GetFranchiseMasterData")
                Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetFranchiseMasterData")
            Return False
        End Try
        Return True
    End Function
End Module
Public Class FranchiseResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of FranchiseMaster)
End Class

Public Class FranchiseMaster
    Public Property FcCd As String
    Public Property FcName As String
    Public Property DelFlg As Integer
End Class
