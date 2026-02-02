# PowerShell script to rename PNG files in Cake folder by adding "_Cake_" prefix
$folderPath = "Art/Items/Cake"

# Check if folder exists
if (!(Test-Path $folderPath)) {
    Write-Host "Folder $folderPath does not exist!"
    exit 1
}

# Get all PNG files in the folder
$pngFiles = Get-ChildItem -Path $folderPath -Filter "*.png"

Write-Host "Found $($pngFiles.Count) PNG files to rename"

# Rename each PNG file
foreach ($file in $pngFiles) {
    $oldName = $file.Name
    $newName = "Item_Cake_" + $oldName.Substring(5) # Remove "Item_" and add "Item_Cake_"
    
    # Check if new name already exists
    if (Test-Path (Join-Path $folderPath $newName)) {
        Write-Host "Warning: $newName already exists, skipping $oldName"
        continue
    }
    
    try {
        Rename-Item -Path $file.FullName -NewName $newName
        Write-Host "Renamed: $oldName -> $newName"
    }
    catch {
        Write-Host "Error renaming $oldName : $($_.Exception.Message)"
    }
}

Write-Host "PNG renaming completed!"

# Now rename the corresponding .meta files
$metaFiles = Get-ChildItem -Path $folderPath -Filter "*.png.meta"

Write-Host "Found $($metaFiles.Count) meta files to rename"

foreach ($file in $metaFiles) {
    $oldName = $file.Name
    $newName = "Item_Cake_" + $oldName.Substring(5) # Remove "Item_" and add "Item_Cake_"
    
    # Check if new name already exists
    if (Test-Path (Join-Path $folderPath $newName)) {
        Write-Host "Warning: $newName already exists, skipping $oldName"
        continue
    }
    
    try {
        Rename-Item -Path $file.FullName -NewName $newName
        Write-Host "Renamed: $oldName -> $newName"
    }
    catch {
        Write-Host "Error renaming $oldName : $($_.Exception.Message)"
    }
}

Write-Host "Meta files renaming completed!"
Write-Host "All renaming completed!"


