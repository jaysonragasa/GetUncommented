# so here's PowerShell version, but it is buggy
# running it:
# .\GetUncommented.ps1 -IncludePaths "C:\Projects\ProjectA", "C:\Projects\ProjectB" -ExcludePaths "bin", "obj", "comonfiles", "newtonsoft.json" -MemberType "public"

param (
    [string[]]$IncludePaths,
    [string[]]$ExcludePaths,
    [string]$MemberType = 'all'
)

# Function to check if a path is excluded
function IsExcluded {
    param (
        [string]$Path
    )
    foreach ($exclude in $ExcludePaths) {
        # Check for the folder name, case insensitive
        if ($Path -like "*\$exclude*") {
            return $true
        }
    }
    return $false
}

$results = @{}

# Iterate through included paths
foreach ($path in $IncludePaths) {
    if (Test-Path $path) {
        Get-ChildItem -Path $path -Recurse -Include *.cs | ForEach-Object {
            $file = $_
            if (-not (IsExcluded $file.FullName)) {
                Write-Host "Processing file: $($file.FullName)"  # Status message
                $content = Get-Content $file.FullName

                # Initialize a list for the file's uncommented members
                $uncommentedMembers = @()

                foreach ($line in $content) {
                    # Trim whitespace from the line
                    $trimmedLine = $line.Trim()

                    # Skip lines that contain comments
                    if ($trimmedLine -match '^\s*//|/\*.*\*/') {
                        continue
                    }

                    # Check for class declarations
                    if ($trimmedLine -match '^(public|private|protected|internal)\s+class\s+(\w+)') {
                        if ($MemberType -eq 'all' -or $MemberType -eq ($trimmedLine -split ' ')[0].ToLower()) {
                            $uncommentedMembers += @{ Member = $trimmedLine; Type = 'Class' }
                        }
                    }
                    # Check for method declarations
                    elseif ($trimmedLine -match '^(public|private|protected|internal)\s+\w+\s+\w+\s*\(.*\)') {
                        if ($MemberType -eq 'all' -or $MemberType -eq ($trimmedLine -split ' ')[0].ToLower()) {
                            $uncommentedMembers += @{ Member = $trimmedLine; Type = 'Method' }
                        }
                    }
                    # Check for property declarations
                    elseif ($trimmedLine -match '^(public|private|protected|internal)\s+\w+\s+\w+\s*{ get; set; }') {
                        if ($MemberType -eq 'all' -or $MemberType -eq ($trimmedLine -split ' ')[0].ToLower()) {
                            $uncommentedMembers += @{ Member = $trimmedLine; Type = 'Property' }
                        }
                    }
                    # Check for enum declarations
                    elseif ($trimmedLine -match '^(public|private|protected|internal)\s+enum\s+(\w+)') {
                        if ($MemberType -eq 'all' -or $MemberType -eq ($trimmedLine -split ' ')[0].ToLower()) {
                            $uncommentedMembers += @{ Member = $trimmedLine; Type = 'Enum' }
                        }
                    }
                }

                # Check if any uncommented members were found
                if ($uncommentedMembers.Count -gt 0) {
                    $results[$file.FullName] = $uncommentedMembers
                } else {
                    Write-Host "No uncommented members found in $($file.FullName)"
                }
            } else {
                Write-Host "Excluded file: $($file.FullName)"
            }
        }
    } else {
        Write-Host "Path not found: $path"
    }
}

# Convert the results to JSON format and output
$outputPath = "nocomments_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
$results | ConvertTo-Json -Depth 5 | Set-Content -Path $outputPath

if ($results.Count -eq 0) {
    Write-Host "No uncommented members found across all included files."
} else {
    Write-Host "Results saved to $outputPath"
}
