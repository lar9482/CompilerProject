$filePath = "./Compiler/obj/Debug/net8.0/Compiler.AssemblyInfo.cs"

$fileContent = Get-Content $filePath -Raw

$CompilerServiceDirective = "using System.Runtime.CompilerServices;`r`n"
$firstAssemblyPattern = '\[assembly: System\.Reflection\.AssemblyCompanyAttribute\("Compiler"\)\]'

# $compilerServicePattern 'using System\.Runtime\.CompilerServices;'
# $compilerServiceMatch = [regex]::Match($fileContent, $compilerServicePattern)
# $compilerTestPattern = '\[assembly: InternalsVisibleTo\("CompilerTests"\)\]'
# if ($compilerServiceMatch.Success) {
#     Write-Host "Compiler directive exists already."
# } else {
#     $firstAssemblyMatch = [regex]::Match($fileContent, $firstAssemblyPattern)
#     if ($firstAssemblyMatch.Success) {
#         $insertIndex = $match.Index
#         $fileContent = $fileContent.Insert($insertIndex-1, $CompilerServiceDirective)
#     }
#     else {
#         Write-Host "Pattern not found."
#     }
# }

# $compilerTestMatch = [regex]::Match($fileContent, $compilerServicePattern)
# if ($compilerTestMatch.Success) {
#     Write-Host "Compiler tests exists already."
# } else {
#     $fileContent += $CompilerTestDirective
# }

$firstAssemblyMatch = [regex]::Match($fileContent, $firstAssemblyPattern)
if ($firstAssemblyMatch.Success) {
    $insertIndex = $match.Index
    $fileContent = $fileContent.Insert($insertIndex, $CompilerServiceDirective)
}
else {
    Write-Host "Pattern not found."
}

$fileContent += "`r`n[assembly: InternalsVisibleTo(`"CompilerTests`")]"
$fileContent | Set-Content $filePath -Force