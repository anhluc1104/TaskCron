Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions

Public Module cron_get_retela_maker_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Try
            Dim targetCount = 0
            Dim executeCount = 0
            Dim responce = RetelaAPI.GetMakerMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of MakerResponce)(responce)
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    log.LogPut(CommonConst.LogLever_Notice, "GetMakerMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetMakerMasterData")
                    PageDataSet.M_Maker.Clear()
                    Using ta As New dsCronTableAdapters.M_MakerTableAdapter(connectionString)
                        Dim idCode = CInt(ta.MaxIDQuery) + 1
                        For Each item In list
                            If ta.FillByRetelaMakerCd(PageDataSet.M_Maker, item.MakerCd) = 1 Then
                                With PageDataSet.M_Maker.Last
                                    .update_date = currentTime
                                    .del_flg = item.DelFlg
                                    .draft_flg = 0
                                    .maker_name = item.MakerName
                                    .retela_maker_code = item.MakerCd
                                End With
                            Else
                                Dim dr = PageDataSet.M_Maker.NewM_MakerRow
                                With dr
                                    .id_code = idCode
                                    .maker_name = item.MakerName
                                    .retela_maker_code = item.MakerCd
                                    .sort_num = idCode
                                    .draft_flg = 0
                                    .del_flg = item.DelFlg
                                    .update_date = currentTime
                                    .update_user = ""
                                End With
                                PageDataSet.M_Maker.AddM_MakerRow(dr)
                                idCode += 1
                            End If
                            executeCount += 1
                        Next
                    End Using
                    If DataUpdate(connectionString) Then
                    End If
                End If
                Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAから中分類マスタの差分取り込みが完了しました。"
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetMakerMasterData " & logMessage, "GetMakerMasterData")
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetMakerMasterData succesfully!", "GetMakerMasterData")
            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetMakerMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetMakerMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetMakerMasterData")
        End Try

    End Sub
    Private Function DataUpdate(connectionString As String) As Boolean
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim sqlCommand = ""
        Dim updateFlg = True
        Try
            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_MakerTableAdapter(connectionString)
                    sqlCommand = ta.GetUpdateCommandText()
                    log.LogPut(CommonConst.LogLever_Notice, sqlCommand, "GetMakerMasterData")
                    If ta.Update(PageDataSet.M_Maker) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                For Each row In PageDataSet.M_Maker
                    Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                        + "], maker_name: [" + row("maker_name").ToString() _
                        + "], retela_maker_code: [" + row("retela_maker_code").ToString() _
                        + "], sort_num: [" + row("sort_num").ToString() _
                        + "], draft_flg: [" + row("draft_flg").ToString() _
                        + "], update_user: [" + row("update_date").ToString() _
                        + "], update_date: [" + row("update_date").ToString() _
                        + "], del_flg: [" + row("del_flg").ToString() + "]"
                    log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetMakerMasterData")
                    log.LogPut(CommonConst.LogLever_Notice, "Executed sql successful!", "GetMakerMasterData")
                Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetMakerMasterData")
            Return False
        End Try
        Return True
    End Function
End Module
Public Class MakerResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of MakerMaster)
End Class
Public Class MakerMaster
    Public Property MakerCd As Integer
    Public Property MakerName As String
    Public Property DelFlg As Integer
End Class