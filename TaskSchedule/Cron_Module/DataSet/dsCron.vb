
Partial Class dsCron
End Class
Namespace dsCronTableAdapters
    Partial Class M_CategoryLTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
    Partial Class M_CategoryMTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
    Partial Class M_CategorySTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
    Partial Class M_FranchiseTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
    Partial Class M_ShopTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
    Partial Class M_MakerTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
    Partial Class M_QuoteProductTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
    Partial Class M_SupplierTableAdapter

        Public Sub New(ByVal conStr As String)
            Me.Connection.ConnectionString = conStr
        End Sub
        Public Function GetUpdateCommandText() As String
            If Me.Adapter.UpdateCommand IsNot Nothing Then
                Return Me.Adapter.UpdateCommand.CommandText
            End If
            Return ""
        End Function
        Public Function GetInsertCommandText() As String
            If Me.Adapter.InsertCommand IsNot Nothing Then
                Return Me.Adapter.InsertCommand.CommandText
            End If
            Return ""
        End Function
    End Class
End Namespace