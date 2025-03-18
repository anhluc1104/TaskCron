Imports System.IO
Imports System.Text

Public Class LogUtil
    Private logFilePath As String

    Dim logDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
    Dim batchDir As String = Path.Combine(logDir, "batch")
    Public Sub New(Optional cid As String = Nothing)

    End Sub
    Private Function GetLogFileBatch(cid As String) As String

        Dim customerId As String = If(cid IsNot Nothing AndAlso cid.Trim().Replace(" ", "").Replace("　", "") <> "", cid.ToString(), "nocid")
        Dim currentDate As Date = DateTime.Now
        Dim dirPath As String = Path.Combine(batchDir, customerId, currentDate.ToString("yyyy"), currentDate.ToString("MM"))
        If Not Directory.Exists(dirPath) Then
            Directory.CreateDirectory(dirPath)
        End If
        Dim logFile As String = Path.Combine(dirPath, currentDate.ToString("yyyyMMdd") & ".Log")
        If Not File.Exists(logFile) Then
            File.Create(logFile).Close()
        End If
        Return logFile
    End Function
    Public Sub LogPut(ByVal Lever As String, ByVal message As String, ByVal cid As String, Optional ByVal encoding As Encoding = Nothing)
        logFilePath = GetLogFileBatch(cid)
        If String.IsNullOrEmpty(logFilePath) Then Return

        Dim dateTime As String = Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        Dim scriptName As String = "VFS_Manage_Cron"
        Dim outputMessage As String = If(encoding IsNot Nothing, encoding.GetString(Encoding.UTF8.GetBytes(message)), message)

        Dim success As Boolean = False
        Dim attempts As Integer = 5

        For i As Integer = 1 To attempts
            Try
                ' Mở file ở chế độ append (thêm vào cuối file)
                Using fs As New FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)
                    Using sw As New StreamWriter(fs)
                        ' Thêm nội dung mới vào cuối file
                        sw.WriteLine($"[{dateTime}] [{scriptName}] [{Lever}] ""{outputMessage}""")
                    End Using
                End Using
                success = True
                Exit For ' Thoát vòng lặp nếu thành công
            Catch ex As IOException
                Threading.Thread.Sleep(500) ' Chờ 500ms trước khi thử lại
            End Try
        Next
    End Sub
End Class
