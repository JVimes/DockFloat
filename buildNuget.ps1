param($SolutionDir, $ProjectPath, $ConfigurationName)

if($ConfigurationName -ne "Release"){ return }

$SolutionDir = $SolutionDir.Trim()
$ProjectPath = $ProjectPath.Trim()

Write-Host "Building NuGet package $ConfigurationName"
$nuget = "$SolutionDir\packages\NuGet.CommandLine.4.*\tools\NuGet.exe"
& $nuget pack $ProjectPath