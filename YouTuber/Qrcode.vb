Public Class Qrcode

    Private Sub Qrcode_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            WebBrowser1.Navigate(My.Settings.qr)
        Catch ex As Exception

        End Try

    End Sub
End Class