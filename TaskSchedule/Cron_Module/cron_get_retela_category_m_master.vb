Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions

Public Module cron_get_retela_category_m_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Dim targetCount = 0
        Dim executeCount = 0
        Try
            Dim reponceStr As String = RetelaAPI.GetCatagoryMMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of CatagoryMResponce)(reponceStr)
            Dim result = ret.Result
            Dim message = ret.Message
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    'log.LogPut(CommonConst.LogLever_Notice, "GetCategoryMMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetCategoryMMasterData")
                    PageDataSet.M_CategoryM.Clear()
                    Using ta As New dsCronTableAdapters.M_CategoryMTableAdapter(connectionString)
                        Dim idCode = CInt(ta.MaxIDQuery) + 1
                        For Each item In list
                            If ta.FillBy(PageDataSet.M_CategoryM, item.DaiCd, item.CyuCd) > 0 Then
                                With PageDataSet.M_CategoryM.Last
                                    .update_date = currentTime
                                    .del_flg = item.DelFlg
                                    .draft_flg = 0
                                    .category_name = item.CyuName
                                    .DaiCd = item.DaiCd
                                    .CyuCd = item.CyuCd
                                End With
                            Else
                                Dim dr = PageDataSet.M_CategoryM.NewM_CategoryMRow
                                With dr
                                    .id_code = idCode
                                    .category_name = item.CyuName
                                    .DaiCd = item.DaiCd
                                    .CyuCd = item.CyuCd
                                    .sort_num = idCode
                                    .draft_flg = 0
                                    .del_flg = item.DelFlg
                                    .update_date = currentTime
                                    .update_user = ""
                                    .comment = ""
                                End With
                                PageDataSet.M_CategoryM.AddM_CategoryMRow(dr)
                                idCode += 1
                            End If
                            executeCount += 1
                        Next
                    End Using
                    If DataUpdate(connectionString) Then
                    End If
                End If
                'Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAから中分類マスタの差分取り込みが完了しました。"
                'log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategoryLMasterData " & logMessage, "GetCategoryMMasterData")
                'log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategoryLMasterData succesfully!", "GetCategoryMMasterData")
            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetCategoryLMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetCategoryMMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetCategoryMMasterData")
        End Try
    End Sub
    Private Function DataUpdate(connectionString As String) As Boolean
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim sqlCommand = ""
        Dim updateFlg = True
        Try
            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_CategoryMTableAdapter(connectionString)
                    'sqlCommand = ta.GetUpdateCommandText()
                    'log.LogPut(CommonConst.LogLever_Notice, sqlCommand, "GetCategoryMMasterData")
                    If ta.Update(PageDataSet.M_CategoryM) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                'For Each row In PageDataSet.M_CategoryM
                '    'Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                '    '    + "], DaiCd: [" + row("DaiCd").ToString() _
                '    '    + "], CyuCd: [" + row("CyuCd").ToString() _
                '    '    + "], category_name: [" + row("category_name").ToString() _
                '    '    + "], comment: [" + row("comment").ToString() _
                '    '    + "], sort_num: [" + row("sort_num").ToString() _
                '    '    + "], draft_flg: [" + row("draft_flg").ToString() _
                '    '    + "], update_user: [" + row("update_date").ToString() _
                '    '    + "], update_date: [" + row("update_date").ToString() _
                '    '    + "], del_flg: [" + row("del_flg").ToString() + "]"
                '    'log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetCategoryMMasterData")
                '    'log.LogPut(CommonConst.LogLever_Notice, "Executed sql successful!", "GetCategoryMMasterData")
                'Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetCategoryMMasterData")
            Return False
        End Try
        Return True
    End Function
End Module
Public Class CatagoryMResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of CatagoryMMaster)
End Class
Public Class CatagoryMMaster
    Public Property DaiCd As Integer
    Public Property CyuCd As Integer
    Public Property CyuName As String
    Public Property TurnNo As Integer
    Public Property DelFlg As Integer
End Class
