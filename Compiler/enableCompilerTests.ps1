$filePath = "./Compiler/obj/Debug/net8.0/Compiler.AssemblyInfo.cs"
# $filePath = "./obj/Debug/net8.0/Compiler.AssemblyInfo.cs"
$fileContent = Get-Content $filePath -Raw

$compilerServiceDirective = "using System.Runtime.CompilerServices;`r`n"
$firstAssembly = '[assembly: System.Reflection.AssemblyCompanyAttribute("Compiler")]'
$compilerTestsAssembly = '[assembly: InternalsVisibleTo("CompilerTests")]'

if ($fileContent.IndexOf($compilerServiceDirective) -eq -1) {
    $firstAssemblyIndex = $fileContent.IndexOf($firstAssembly + "`r`n")
    $fileContent = $fileContent.Insert($firstAssemblyIndex, $CompilerServiceDirective)
}

if ($fileContent.IndexOf($compilerTestsAssembly) -eq -1) {
    $fileContent += "`r`n"
    $fileContent += $compilerTestsAssembly
}
$fileContent | Set-Content $filePath -Force