# PowerShell script to rename files in Fruit folder
# Change from "Item_Fruit_Fruit_XXX" to "Item_Fruit_XXX"
$folderPath = "Art/Items/Fruit"

# Check if folder exists
if (!(Test-Path $folderPath)) {
    Write-Host "Folder $folderPath does not exist!"
    exit 1
}

# Get all files in the folder
$files = Get-ChildItem -Path $folderPath -File

Write-Host "Found $($files.Count) files to rename"

# Rename each file
foreach ($file in $files) {
    $oldName = $file.Name
    
    # Check if the file already has the correct format
    if ($oldName -match "^Item_Fruit_Fruit_(.+)$") {
        $newName = "Item_Fruit_" + $matches[1]
        
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
    } else {
        Write-Host "Skipping $oldName (doesn't match expected pattern)"
    }
}

Write-Host "Renaming completed!"
