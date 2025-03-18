Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Configuration
Imports Newtonsoft.Json
Public Module RetelaAPI
    Public Function GetFranchiseMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetFranchiseMasterData", param)
    End Function

    Public Function GetShopMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetShopMasterData", param)
    End Function

    Public Function GetQuoteProductMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetQuoteProductMasterData", param)
    End Function

    Public Function GetMakerMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetMakerMasterData", param)
    End Function

    Public Function GetCatagoryLMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetCategoryLMasterData", param)
    End Function
    Public Function GetCatagoryMMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetCategoryMMasterData", param)
    End Function
    Public Function GetCatagorySMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetCategorySMasterData", param)
    End Function

    Public Function GetSupplierMasterData(Optional param As String = "") As String
        Return GetRetelaData("GetSupplierMasterData", param)
    End Function

    Private Function GetRetelaData(apiName As String, Optional param As String = "") As String
        Dim now = Date.Now.ToString("yyyy/MM/dd HH:mm:ss")
        Dim url = ConfigurationManager.AppSettings("RETELA_URL") & apiName
        Dim checkDatFolder As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dat")
        Dim checkFolder As String = Path.Combine(checkDatFolder, "retela_update")
        Dim checkFile As String = Path.Combine(checkFolder, apiName & ".dat")

        Dim runFlg As Boolean = True

        If Not File.Exists(checkDatFolder) Then
            Directory.CreateDirectory(checkDatFolder)
        End If

        If Not File.Exists(checkFolder) Then
            Directory.CreateDirectory(checkFolder)
        End If

        If Not File.Exists(checkFile) Then
            File.Create(checkFile).Dispose()
            runFlg = False
        End If

        Dim last_update_date As String = ""

        If runFlg Then
            ' Mở file để đọc với FileShare.ReadWrite
            Using fs As New FileStream(checkFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using sr As New StreamReader(fs)
                    last_update_date = sr.ReadToEnd().Trim()
                End Using
            End Using
        Else
            last_update_date = ConfigurationManager.AppSettings("LAST_UPDATE_DATETIME")
        End If

        Dim authenticationKey = GetAuthenticationKey()
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim manageCd = ConfigurationManager.AppSettings("RETELA_MANAGE_CD")
        Dim data As New Dictionary(Of String, String) From {
            {"contractClientCd", clientCd},
            {"manageCd", manageCd},
            {"authenticationKey", authenticationKey},
            {"lastUpdateDateTime", last_update_date}
        }

        Dim ret As String = PostApiCron(url, data, apiName)

        Dim success As Boolean = False
        Dim attempts As Integer = 5

        For i As Integer = 1 To attempts
            Try
                Using fs As New FileStream(checkFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)
                    Using sw As New StreamWriter(fs)
                        sw.WriteLine(now)
                    End Using
                End Using
                success = True
                Exit For ' Thoát vòng lặp nếu thành công
            Catch ex As IOException
                Threading.Thread.Sleep(500) ' Chờ 500ms trước khi thử lại
            End Try
        Next

        Return ret

    End Function

    Private Function GetAuthenticationKey() As String
        Dim today = Now.Date.ToString("yyyy/MM/dd")
        Dim clientCd = ConfigurationManager.AppSettings("RETELA_CONTRACT_CLIENT_CD")
        Dim manageCd = ConfigurationManager.AppSettings("RETELA_MANAGE_CD")
        Dim original_string As String = clientCd & manageCd & today

        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(original_string)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Dim authenticationKey As String = BitConverter.ToString(hash).Replace("-", "").ToUpper()
            Return authenticationKey
        End Using
    End Function

    Private Function PostApiCron(url As String, data As Dictionary(Of String, String), apiName As String) As String
        Dim log As New LogUtil()
        Try
            Using client As New HttpClient()
                client.Timeout = TimeSpan.FromSeconds(600)
                Dim content As New FormUrlEncodedContent(data)
                Dim response As HttpResponseMessage = client.PostAsync(url, content).Result
                Dim responseString As String = response.Content.ReadAsStringAsync().Result
                'log.LogPut(CommonConst.LogLever_Notice, "response 文字列[" & responseString & "]", apiName)
                If response.IsSuccessStatusCode Then
                Else
                    log.LogPut(CommonConst.LogLever_Error, "Lỗi khi gọi API: " & response.StatusCode, apiName)
                End If
                Return responseString
            End Using
        Catch ex As Exception
            log.LogPut(CommonConst.LogLever_Error, "送信先URL[" & url & "]", apiName)
            log.LogPut(CommonConst.LogLever_Error, "data[" & JsonConvert.SerializeObject(data) & "]", apiName)
            log.LogPut(CommonConst.LogLever_Error, ex.Message, apiName)
            Return Nothing
        End Try
    End Function
End Module
