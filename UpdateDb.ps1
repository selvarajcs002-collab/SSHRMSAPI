$sql = Get-Content 'db_attendance_setup.sql' -Raw
$conn = New-Object System.Data.SqlClient.SqlConnection('Server=200.141.4.172,1433;Database=SSManagement_HRMS;User Id=sa;Password=SelVaWinNrR1@;TrustServerCertificate=True;Encrypt=True')
$conn.Open()

$commands = $sql -split 'GO\s*'
foreach ($cmdText in $commands) {
    if ($cmdText.Trim().Length -gt 0) {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $cmdText
        $cmd.ExecuteNonQuery() | Out-Null
    }
}
$conn.Close()
Write-Host "Database updated successfully!"
