Public Class Loadin

    Private Sub Loadin_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Label3.Text = My.Settings.input
        Label5.Text = My.Settings.outputx
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        My.Settings.abort = True
        Try
            Main.BackgroundWorker1.CancelAsync()

        Catch
        End Try
        Me.Close()
    End Sub
End Class