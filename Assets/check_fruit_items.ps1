# PowerShell script to check PNG files in Fruit folder and generate enum entries
$folderPath = "Art/Items/Fruit"

# Check if folder exists
if (!(Test-Path $folderPath)) {
    Write-Host "Folder $folderPath does not exist!"
    exit 1
}

# Get all PNG files in the folder and extract base names
$pngFiles = Get-ChildItem -Path $folderPath -Filter "*.png" | ForEach-Object { $_.BaseName }

Write-Host "Found $($pngFiles.Count) PNG files in Fruit folder:"
$pngFiles | Sort-Object | ForEach-Object { Write-Host "  $_" }

# Generate enum entries starting from 600
Write-Host "`nEnum entries to add (starting from 600):"
$nextValue = 600
foreach ($pngFile in ($pngFiles | Sort-Object)) {
    Write-Host "        $pngFile = $nextValue,"
    $nextValue++
}

Write-Host "`nTotal enum entries to add: $($pngFiles.Count)"
Write-Host "Value range: 600 to $($nextValue - 1)"


