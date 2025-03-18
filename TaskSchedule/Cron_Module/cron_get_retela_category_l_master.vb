Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions
Public Module cron_get_retela_category_l_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Dim targetCount = 0
        Dim executeCount = 0
        Try
            Dim reponceStr As String = RetelaAPI.GetCatagoryLMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of CatagoryLResponce)(reponceStr)
            Dim result = ret.Result
            Dim message = ret.Message
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    'log.LogPut(CommonConst.LogLever_Notice, "GetCategoryLMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetCategoryLMasterData")
                    PageDataSet.M_CategoryL.Clear()
                    Using ta As New dsCronTableAdapters.M_CategoryLTableAdapter(connectionString)
                        For Each item In list
                            If ta.FillBy(PageDataSet.M_CategoryL, item.DaiCd) = 1 Then
                                With PageDataSet.M_CategoryL.Last
                                    .update_date = currentTime
                                    .del_flg = item.DelFlg
                                    .draft_flg = 0
                                    .category_name = item.DaiName
                                    .retela_dai_code = item.DaiCd
                                End With
                            Else
                                Dim dr = PageDataSet.M_CategoryL.NewM_CategoryLRow
                                With dr
                                    .id_code = item.DaiCd
                                    .category_name = item.DaiName
                                    .retela_dai_code = item.DaiCd
                                    .sort_num = item.DaiCd
                                    .draft_flg = 0
                                    .del_flg = item.DelFlg
                                    .update_date = currentTime
                                    .update_user = ""
                                    .comment = ""
                                End With
                                PageDataSet.M_CategoryL.AddM_CategoryLRow(dr)
                            End If
                            executeCount += 1
                        Next
                    End Using
                    If DataUpdate(connectionString) Then
                    End If
                End If
                'Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAから大分類マスタの差分取り込みが完了しました。"
                'log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategoryLMasterData " & logMessage, "GetCategoryLMasterData")
                'log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategoryLMasterData succesfully!", "GetCategoryLMasterData")
            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategoryLMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetCategoryLMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetCategoryLMasterData")
        End Try
    End Sub
    Private Function DataUpdate(connectionString As String) As Boolean
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim updateFlg = True
        Try
            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_CategoryLTableAdapter(connectionString)
                    'log.LogPut(CommonConst.LogLever_Notice, ta.GetUpdateCommandText(), "GetCategoryLMasterData")
                    If ta.Update(PageDataSet.M_CategoryL) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                'For Each row In PageDataSet.M_CategoryL
                '    'Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                '    '    + "], category_name: [" + row("category_name").ToString() _
                '    '    + "], retela_dai_code: [" + row("retela_dai_code").ToString() _
                '    '    + "], comment: [" + row("comment").ToString() _
                '    '    + "], sort_num: [" + row("sort_num").ToString() _
                '    '    + "], draft_flg: [" + row("draft_flg").ToString() _
                '    '    + "], update_user: [" + row("update_date").ToString() _
                '    '    + "], update_date: [" + row("update_date").ToString() _
                '    '    + "], del_flg: [" + row("del_flg").ToString() + "]"
                '    'log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetCategoryLMasterData")
                '    'log.LogPut(CommonConst.LogLever_Notice, "Executed sql successful!", "GetCategoryLMasterData")
                'Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetCategoryLMasterData")
            Return False
        End Try
        Return True
    End Function
End Module
Public Class CatagoryLResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of CatagoryLMaster)
End Class
Public Class CatagoryLMaster
    Public Property DaiCd As Integer
    Public Property DaiName As String
    Public Property TurnNo As Integer
    Public Property DelFlg As Integer

End Class
