Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class Main
    Public request As HttpWebRequest
    Public request2 As HttpWebRequest
    Public response As HttpWebResponse
    Public source As String
    Public response2 As HttpWebResponse
    Public source2 As String
    Public WithEvents web As New WebClient
    Public downloadurl As String
    Delegate Sub OneArgD(ByVal arg As Object)
    Dim signature As String
    Dim filetype As String
    Dim proc As New Process
    Dim out As String
    Dim down As String
#Region "Generate You-Tube URL"
    Public Sub MakeDownloadURL(ByVal url As String, ByRef outputa As String, Optional ByRef type As String = "", Optional ByRef size As Integer = 0)
        'PERFORMS CLEANUP
        downloadurl = ""
        If request IsNot Nothing Then
            Try
                request.Abort()
            Catch
            End Try
        End If
        If response IsNot Nothing Then
            Try
                response.Close()
            Catch
            End Try
        End If
        Try
            source = ""
        Catch
        End Try

        request = DirectCast(HttpWebRequest.Create(url.ToString), HttpWebRequest)
        Try
            response = DirectCast(request.GetResponse, HttpWebResponse)
        Catch
            type = ""
            Exit Sub
        End Try
        source = New StreamReader(response.GetResponseStream, System.Text.Encoding.Default).ReadToEnd

        If source.IndexOf("video_id") > -1 Then
            'This grabs the .flv location URL ready for doownloading.
            signature = source
            If signature.Contains("&amp;fmt_url_map=") = True Then
                Try
                    signature = System.Text.RegularExpressions.Regex.Split(signature, "&amp;fmt_url_map=")(1)
                Catch
                End Try
            ElseIf signature.Contains(Chr(34) + "fmt_url_map" + Chr(34) + ": " + Chr(34)) = True Then
                Try
                    signature = System.Text.RegularExpressions.Regex.Split(signature, Chr(34) + "fmt_url_map" + Chr(34) + ": " + Chr(34))(1)
                Catch
                End Try
            Else
                Debug.WriteLine("Critical Error: Download URL could not be produced.")
                type = ""
                Exit Sub
            End If

            'This changes the URL to conform to internet standards eg. %20 = " "
            signature = signature.Remove(0, 5)
            signature = signature.Replace("%252C", ",")
            signature = signature.Replace("%2F", "/")
            signature = signature.Replace("%3D", "=")
            signature = signature.Replace("%3F", "?")
            signature = signature.Replace("%3A", ":")
            signature = signature.Replace("%26", "&")
            Dim sig2 As String = signature
            sig2 = sig2.IndexOf("http", 30)
            signature = signature.Remove(sig2, signature.Length - sig2)
            signature = signature.Remove(signature.IndexOf("%"), 7)
            'Removes any remaining % characters from the string.
            signature = signature.Replace("%", "")

            'Checks for and fixes any malformed http requests in the string.
            If signature.StartsWith("ttp://") Then
                signature = signature.Replace("ttp://", "http://")
            End If

            'Checks for and Removes any remaining C characters from the string.
            If signature.Chars(signature.Length - 1) = "C" Then
                signature = signature.Remove(signature.Length - 1, 1)
            End If

            'Checks for videos such as http://www.youtube.com/watch?v=LvLCvdeWcsw that would not download due to a new layout of download data.
            If signature.Contains("rv.2.rating") Then
                Dim n As Integer = signature.IndexOf(ChrW(34) + ",")
                signature = signature.Remove(n, signature.Length - n)
            End If

            'PERFORMS CLEANUP
            If request2 IsNot Nothing Then
                Try
                    request2.Abort()
                Catch
                End Try
            End If
            If response2 IsNot Nothing Then
                Try
                    response2.Close()
                Catch
                End Try
            End If

            Try
                request2 = DirectCast(HttpWebRequest.Create(signature), HttpWebRequest)
            Catch
                type = ""
                Exit Sub
            End Try
            response2 = DirectCast(request2.GetResponse, HttpWebResponse)
            If response2.ContentType = "video/x-flv" Then
                type = ".flv"
                outputa = signature
                size = response2.ContentLength
            ElseIf response2.ContentType = "video/mp4" Then
                type = ".mp4"
                outputa = signature
                size = response2.ContentLength
            Else
                type = ""
                outputa = ""
                Exit Sub
            End If
        End If

    End Sub
#End Region

    Function startConversion()
        Control.CheckForIllegalCrossThreadCalls = False
        Dim input As String = Me.OpenF.FileName

        My.Settings.abort = False

        Dim exepath As String = Application.StartupPath + "\ffmpeg.exe"
        Dim quality As Integer

        If qal.Text = "Low" Then
            quality = "8"
        End If
        If qal.Text = "Medium" Then
            quality = "16"
        End If
        If qal.Text = "Optimal" Then
            quality = "24"
        End If
        If qal.Text = "High" Then
            quality = "30"
        End If

        Dim res As String = ""
        Dim codec As String = ""
        Dim rate As String = ""
        Dim brate As String = ""
        Dim ab As String = ""
        Dim ac As String = ""

        If tovid.Text = "PSP Video (H.264 MP4)" Then
            res = " -s "
            res = res & "368x208"
            codec = " -vcodec "
            codec = codec & "libx264 "
        End If
        If tovid.Text = "iPhone Video (MPEG-4 MP4)" Then
            codec = " -vcodec "
            codec = codec & "libx264 "
        End If


        Dim cmd As String = " -i """ + input + """ -ar 22050 -qscale " & quality & codec & res & " -y """ + out + """" 'ffmpeg commands -y replace
        Dim startinfo As New System.Diagnostics.ProcessStartInfo
        Dim sr As StreamReader

        If tovid.Text = "Cell Phone (H.263 3GP)" Then
            cmd = " -i " & input & " -vcodec h263 -qscale 15 -s 176x144 -r 12 -b 30k -ar 8000 -ab 12.2k -ac 1 -y " & out 'ffmpeg commands -y replace
        End If

        Dim ffmpegOutput As String

        ' all parameters required to run the process
        startinfo.FileName = exepath
        startinfo.Arguments = cmd
        startinfo.UseShellExecute = False
        startinfo.WindowStyle = ProcessWindowStyle.Hidden
        startinfo.RedirectStandardError = True
        startinfo.RedirectStandardOutput = True
        startinfo.CreateNoWindow = True
        proc.StartInfo = startinfo
        proc.Start() ' start the process



        sr = proc.StandardError 'standard error is used by ffmpeg
        Me.Button5.Enabled = False
        Do Until proc.HasExited
            ffmpegOutput = sr.ReadLine
            If BackgroundWorker1.CancellationPending Then 'check if a cancellation request was made
                Exit Function
            End If

        Loop

        Me.Button5.Enabled = True
        Return 0
    End Function

    Private Sub Main_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        If Clipboard.ContainsText = True Then
            If Clipboard.GetText.Contains("youtube.com/watch?v=") Then
                If Clipboard.GetText.StartsWith("http://www.") = True Then
                    txturl.Text = Clipboard.GetText
                    TabControl1.Focus()
                Else
                    If Clipboard.GetText.StartsWith("http://") = True Then
                        Dim str As String = Clipboard.GetText
                        str = Replace(str, "http://", "http://www.")
                        txturl.Text = str
                        TabControl1.Focus()
                    Else
                        If Clipboard.GetText.StartsWith("www.") = True Then
                            txturl.Text = "http://" & Clipboard.GetText
                            TabControl1.Focus()
                        Else
                            txturl.Text = "http://www." & Clipboard.GetText
                            TabControl1.Focus()
                        End If


                    End If

                End If
                

            End If
        End If
        txturl.DeselectAll()
        TabControl1.Focus()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        filetype = ""
        Dim size As Integer = 0
        'to download youtube video...
        Try
            If txturl.Text.Contains("youtube.com/watch?v=") = True Then
                MakeDownloadURL(txturl.Text, downloadurl, filetype, size)


                WebBrowser1.Navigate(signature)

            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Function UrlIsValid(ByVal url As String) As Boolean
        Dim is_valid As Boolean = False
        If url.ToLower().StartsWith("www.") Then url = _
            "http://" & url

        Dim web_response As HttpWebResponse = Nothing
        Try
            Dim web_request As HttpWebRequest = _
                HttpWebRequest.Create(url)
            web_response = _
                DirectCast(web_request.GetResponse(),  _
                HttpWebResponse)
            Return True
        Catch ex As Exception
            Return False
        Finally
            If Not (web_response Is Nothing) Then _
                web_response.Close()
        End Try
    End Function

    Private Sub Main_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Clipboard.Clear()
        tovid.Text = "Windows Media Video (V.7 WMV)"
        qal.Text = "High"
        WebBrowser2.Navigate(Application.StartupPath & "\player.swf")
    End Sub

    Private Sub txturl_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles txturl.Click
        txturl.SelectAll()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        filetype = ""
        Dim size As Integer = 0
        'to download youtube video...
        Try
            If txturl.Text.Contains("youtube.com/watch?v=") = True Then

                Qrcode.ShowDialog()

            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        startConversion()
    End Sub

    Private Sub TabPage1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TabPage1.Click
        txturl.DeselectAll()
        TabControl1.Focus()
    End Sub
    Public Shared Function Shorten(ByVal url As String) As String
        Dim post As String = "{""longUrl"": """ & url & """}"
        Dim shortUrl As String = url
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url"), HttpWebRequest)
        Try
            request.ServicePoint.Expect100Continue = False
            request.Method = "POST"
            request.ContentLength = post.Length
            request.ContentType = "application/json"
            request.Headers.Add("Cache-Control", "no-cache")
            Using requestStream As Stream = request.GetRequestStream()
                Dim postBuffer As Byte() = Encoding.ASCII.GetBytes(post)
                requestStream.Write(postBuffer, 0, postBuffer.Length)
            End Using
            Using response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
                Using responseStream As Stream = response.GetResponseStream()
                    Using responseReader As New StreamReader(responseStream)
                        Dim json As String = responseReader.ReadToEnd()
                        shortUrl = Regex.Match(json, """id"": ?""(?<id>.+)""").Groups("id").Value
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' if Google's URL Shortner is down...
            System.Diagnostics.Debug.WriteLine(ex.Message)
            System.Diagnostics.Debug.WriteLine(ex.StackTrace)
        End Try
        Return shortUrl
    End Function
    Private Sub txturl_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txturl.TextChanged
        If txturl.Text.Contains("youtube.com/watch?v=") = True Then
            If UrlIsValid(txturl.Text) = True Then
                Try
                    Dim urlQueryStrPos As Integer
                    TabControl1.Visible = False

                    'Lets find the index of the query string ?v=
                    'right after the equal sign would be the youtube video id which is
                    'an 11-character string generated by youtube when a user uploads a video.
                    urlQueryStrPos = Me.txturl.Text.IndexOf("?v=")

                    If urlQueryStrPos < 0 Then MessageBox.Show("Please enter a valid url.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Exclamation) : Exit Sub

                    Dim youTubeVideoIdStartPos As Integer
                    youTubeVideoIdStartPos = urlQueryStrPos + 3         'locate the start position of the video ID

                    Dim youtubeVideoId As String
                    youtubeVideoId = Me.txturl.Text.Substring(youTubeVideoIdStartPos, 11) 'extract the video id from the url string

                    Dim smallThumbnailUrl As String

                    'The Thumbnail Images can be previewed by visiting the following URLS:
                    'http://img.youtube.com/vi/<videoid>/default.jpg
                    '
                    'Just replace <videoid> with the real video id
                    'default.jpg - default thumbnail
                    '0.jpg - large image preview (480px x 360px)
                    '1.jpg - Alternate thumbnail
                    '3.jpg - Alternate thumbnail
                    smallThumbnailUrl = "http://img.youtube.com/vi/" & youtubeVideoId & "/default.jpg"
                    Me.thumb.Load(smallThumbnailUrl)

                    Dim size As Integer = 0
                    MakeDownloadURL(txturl.Text, downloadurl, filetype, size)

                    Dim shorta As String = Shorten(signature)
                    shortu.Text = shorta
                    My.Settings.qr = "http://chart.apis.google.com/chart?cht=qr&chs=130x130&chl=" & shorta & "&chld=H|0"
                    TabControl1.Visible = True

                    txturl.DeselectAll()

                Catch ex As Exception

                End Try
            End If

        Else
            If txturl.Text = "" Then

                shortu.Text = ""
                Me.thumb.Image = Nothing
            Else
                Me.thumb.Image = My.Resources.defaulth
            End If


        End If

    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        If from.Text = "" Or from.Text <> OpenF.FileName Then
            MsgBox("Select a file to convert", MsgBoxStyle.Information, "Select a file")
            Exit Sub
        End If
        My.Settings.input = from.Text
        out = from.Text
        If out.Contains(".wmv") = True Then
            out = out.Replace(".wmv", "")
        End If
        If out.Contains(".avi") = True Then
            out = out.Replace(".avi", "")
        End If
        If out.Contains(".mov") = True Then
            out = out.Replace(".mov", "")
        End If
        If out.Contains(".mp4") = True Then
            out = out.Replace(".mp4", "")
        End If
        If out.Contains(".3gp") = True Then
            out = out.Replace(".3gp", "")
        End If
        If out.Contains(".flv") = True Then
            out = out.Replace(".flv", "")
        End If
        If out.Contains(".vob") = True Then
            out = out.Replace(".vob", "")
        End If
        If out.Contains(".flv") = True Then
            out = out.Replace(".flv", "")
        End If

        If tovid.Text = "Windows Media Video (V.7 WMV)" Then
            out = out & ".wmv"
        End If
        If tovid.Text = "Xvid MPEG-4 Codec (AVI)" Then
            out = out & ".avi"
        End If
        If tovid.Text = "iPod Video (Apple QuickTime MOV)" Then
            out = out & ".mov"
        End If
        If tovid.Text = "iPhone Video (MPEG-4 MP4)" Then
            out = out & ".mp4"
        End If
        If tovid.Text = "PSP Video (H.264 MP4)" Then
            out = out & ".mp4"
        End If
        If tovid.Text = "Cell Phone (H.263 3GP)" Then
            out = out & ".3gp"
        End If
        If tovid.Text = "Flash Video Codec (FLV)" Then
            out = out & ".flv"
        End If
        If tovid.Text = "MPEG Audio Layer 3 (MP3)" Then
            out = out & ".mp3"
        End If
        My.Settings.outputx = out
        BackgroundWorker1.RunWorkerAsync()
        Loadin.ShowDialog()
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        If OpenF.ShowDialog = Windows.Forms.DialogResult.OK Then
            from.Text = OpenF.FileName
            Button3.Visible = False
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Try
            Process.Start(out)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted

        Button3.Visible = True
        If My.Settings.abort = False Then
            Loadin.Close()
            MsgBox("Conversion of video file completed!", MsgBoxStyle.Information)
        Else
            Loadin.Close()
            Kill(My.Settings.outputx)
            Button1.Visible = False
            MsgBox("Conversion of video file canceled!", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.Click
        About.ShowDialog()
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        If OpenV.ShowDialog = Windows.Forms.DialogResult.OK Then
            If OpenV.FileName.Contains(".flv") = True Then
                Button7.Visible = True
                WebBrowser2.Navigate(Application.StartupPath & "\player.swf?file=" & OpenV.FileName & "&autoplay=true")
            Else
                MsgBox("Only flash videos (.flv) supported!", MsgBoxStyle.Information)
            End If

        End If
    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        Flvplay.ShowDialog()
    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        Help.Show()
    End Sub

    Private Sub Button9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click
        License.ShowDialog()
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        About.ShowDialog()
    End Sub
End Class
