Imports Newtonsoft.Json
Imports System.Configuration
Imports System.Transactions

Public Module cron_get_retela_supplier_master
    Private ReadOnly Property PageDataSet() As New dsCron
    Public Sub CallAPI()
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("SmaPhoDockInfo").ConnectionString
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim currentTime = Now.Date.ToString("yyyy-MM-dd HH:mm:ss")
        Dim targetCount = 0
        Dim executeCount = 0
        Try
            Dim reponceStr As String = RetelaAPI.GetSupplierMasterData("cron")
            Dim ret = JsonConvert.DeserializeObject(Of SupplierResponce)(reponceStr)
            Dim result = ret.Result
            Dim message = ret.Message
            If ret.Result = True Then
                Dim list = ret.List
                If list IsNot Nothing AndAlso list.Count > 0 Then
                    targetCount = list.Count
                    log.LogPut(CommonConst.LogLever_Notice, "GetSupplierMasterData 更新対象データ[" & targetCount.ToString & "]件 更新処理開始", "GetSupplierMasterData")
                    Using ta As New dsCronTableAdapters.M_SupplierTableAdapter(connectionString)
                        For Each item In list

                            PageDataSet.M_Supplier.Clear()
                            If ta.FillBySupplierCd(PageDataSet.M_Supplier, item.SupplierCd) = 1 Then

                                With PageDataSet.M_Supplier.Last
                                    .supplierName = item.SupplierName
                                    .supplierCd = item.SupplierCd
                                    .sort_num = item.TurnNo
                                    .del_flg = item.DelFlg
                                    .update_date = currentTime
                                    .update_user = 0
                                End With

                            Else

                                Dim dr = PageDataSet.M_Supplier.NewM_SupplierRow
                                With dr
                                    .id_code = If(ta.MaxIDQuery Is Nothing, 0, ta.MaxIDQuery) + 1
                                    .supplierName = item.SupplierName
                                    .supplierCd = item.SupplierCd
                                    .sort_num = If(ta.MaxIDQuery Is Nothing, 0, ta.MaxIDQuery) + 1
                                    .draft_flg = 0
                                    .del_flg = item.DelFlg
                                    .update_date = currentTime
                                    .update_user = 0
                                End With
                                PageDataSet.M_Supplier.AddM_SupplierRow(dr)
                            End If

                            If DataUpdate(connectionString) Then
                                executeCount += 1
                            End If

                        Next
                    End Using
                End If
                Dim logMessage = "ReTELAからの応答データ件数 [" & targetCount & "]件 処理対象件数 [" & executeCount & "]件 ReTELAから仕入先マスタの差分取り込みが完了しました。"
                log.LogPut(CommonConst.LogLever_Notice, "GetSupplierMasterData " & logMessage, "GetSupplierMasterData")
                log.LogPut(CommonConst.LogLever_Notice, "GetSupplierMasterData succesfully!", "GetSupplierMasterData")
            Else
                log.LogPut(CommonConst.LogLever_Notice, "cron_GetSupplierMasterData ReTELAからエラーが返却されました。エラーメッセージ [" & ret.Message & "]", "GetSupplierMasterData")
            End If
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetSupplierMasterData")
        End Try
    End Sub

    Private Function DataUpdate(connectionString As String) As Boolean

        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim log As New LogUtil()
        Dim sqlCommand = ""
        Dim updateFlg = True
        Try
            Using trScope As New System.Transactions.TransactionScope(TransactionScopeOption.Required)
                Using ta As New dsCronTableAdapters.M_SupplierTableAdapter(connectionString)
                    sqlCommand = ta.GetUpdateCommandText()
                    If ta.Update(PageDataSet.M_Supplier) < 1 Then
                        Return False
                    Else
                    End If
                End Using
                For Each row In PageDataSet.M_Supplier
                    log.LogPut(CommonConst.LogLever_Notice, sqlCommand, "GetSupplierMasterData")
                    Dim sqlParam = "bind params..id_code: [" + row.id_code.ToString() _
                        + "], supplierCd: [" + row("supplierCd").ToString() _
                        + "], supplierName: [" + row("supplierName").ToString() _
                        + "], sort_num: [" + row("sort_num").ToString() _
                        + "], draft_flg: [" + row("draft_flg").ToString() _
                        + "], update_user: [" + row("update_date").ToString() _
                        + "], update_date: [" + row("update_date").ToString() _
                        + "], del_flg: [" + row("del_flg").ToString() + "]"
                    log.LogPut(CommonConst.LogLever_Notice, sqlParam, "GetSupplierMasterData")
                    log.LogPut(CommonConst.LogLever_Notice, "Connection successful!", "GetSupplierMasterData")
                Next
                trScope.Complete()
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, ex.Message, "GetSupplierMasterData")
            Return False
        End Try
        Return True

    End Function
End Module
Public Class SupplierResponce
    Public Property Result As Boolean
    Public Property Message As String
    Public Property List As List(Of SupplierMaster)
End Class
Public Class SupplierMaster
    Public Property SupplierCd As String
    Public Property SupplierName As String
    Public Property TurnNo As Integer
    Public Property DelFlg As Integer
End Class
