$package = "com.self.budgetmanager"
$dbfile = "budget.sqlite"
$dbBrowser = "C:\Program Files\DB Browser for SQLite\DB Browser for SQLite.exe"

Write-Host "Pulling $dbfile from $package ..."
adb exec-out run-as $package cat files/$dbfile > $dbfile

if (Test-Path $dbBrowser) {
    Write-Host "Opening DB Browser..."
    Start-Process $dbBrowser $dbfile
}
else {
    Write-Host "DB Browser not found. Opening with default program..."
    Start-Process $dbfile
}
