Imports tik4net
Public Class Form1
    Private connection As ITikConnection
    Private monitoringActive As Boolean = False
    Private lastRx As New Dictionary(Of String, Double)
    Private lastTx As New Dictionary(Of String, Double)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        'Setup Listview
        LV1.View = View.Details
        LV1.Columns.Add("Interface", 150)
        LV1.Columns.Add("RX (KB/s)", 120)
        LV1.Columns.Add("TX (KB/s)", 120)
        LV1.FullRowSelect = True
        LV1.GridLines = True
    End Sub

    Private Sub BtnConnect_Click(sender As Object, e As EventArgs) Handles BtnConnect.Click
        Try
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api)
            connection.Open(TxtIP.Text, TxtUser.Text, TxtPass.Text)
            MessageBox.Show("Connected successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LblStatus.Text = "Connected to " & TxtIP.Text

        Catch ex As Exception
            MessageBox.Show("Failed to connect: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub BtnStart_Click(sender As Object, e As EventArgs) Handles BtnStart.Click
        Try
            If connection Is Nothing OrElse Not connection.IsOpened Then
                MessageBox.Show("Please connect to the router first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            monitoringActive = True
            Timer1.Interval = 5000 ' 5 seconds
            Timer1.Start()
            LblStatus.Text = "Status: Monitoring Aktif"
            LblStatus.ForeColor = Color.Green
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        monitoringActive = False
        Timer1.Stop()
        LblStatus.Text = "Status: Monitoring Nonaktif"
        LblStatus.ForeColor = Color.Red
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Not monitoringActive Then Exit Sub

        Try
            Dim cmd = connection.CreateCommand("/interface/print")
            Dim results = cmd.ExecuteList()

            LV1.Items.Clear()

            For Each iface In results
                Dim name As String = iface.Words("name")
                If name.Contains("pppoe") Then
                    Dim rx As Double = CDbl(iface.Words("rx-byte"))
                    Dim tx As Double = CDbl(iface.Words("tx-byte"))

                    'hitung data dari pembacaan sebelummnya
                    Dim rxRate As Double = 0
                    Dim txRate As Double = 0

                    If lastRx.ContainsKey(name) Then
                        rxRate = (rx - lastRx(name)) / 1024 'KB/s
                    End If
                    If lastTx.ContainsKey(name) Then
                        txRate = (tx - lastTx(name)) / 1024 'KB/s
                    End If

                    lastRx(name) = rx
                    lastTx(name) = tx

                    'tambahkan ke listview
                    Dim item As New ListViewItem(name)
                    item.SubItems.Add(rxRate.ToString("0.00") & " KB/s")
                    item.SubItems.Add(txRate.ToString("0.00") & " KB/s")
                    LV1.Items.Add(item)
                End If
            Next
        Catch ex As Exception
            LblStatus.Text = "Error: " & ex.Message
        End Try
    End Sub
End Class
