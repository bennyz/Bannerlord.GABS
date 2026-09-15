[CmdletBinding()]
param(
    [string] $GameDir,
    [switch] $WithoutCalradicExchange
)

$ErrorActionPreference = 'Stop'

if (-not $GameDir) { $GameDir = $env:BANNERLORD_STABLE_DIR }
if (-not $GameDir) { $GameDir = $env:BANNERLORD_GAME_DIR }
if (-not $GameDir) {
    $GameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord'
}
$GameDir = [IO.Path]::GetFullPath($GameDir)

$configPath = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Mount and Blade II Bannerlord\Configs\engine_config.txt'
if (Test-Path -LiteralPath $configPath) {
    $content = [IO.File]::ReadAllText($configPath)
    $patched = [Text.RegularExpressions.Regex]::Replace($content, '(?m)^safely_exited\s*=\s*\d+', 'safely_exited = 1')
    if ($patched -ne $content) {
        [IO.File]::WriteAllText($configPath, $patched, [Text.UTF8Encoding]::new($false))
        Write-Host '[GABS] Reset safely_exited to prevent the Safe Mode dialog'
    }
}

$requiredModules = @(
    'Bannerlord.Harmony',
    'Bannerlord.ButterLib',
    'Bannerlord.UIExtenderEx',
    'Bannerlord.MBOptionScreen',
    'Bannerlord.GABS',
    'Native',
    'SandBoxCore',
    'BirthAndDeath',
    'CustomBattle',
    'Sandbox',
    'StoryMode'
)
if (-not $WithoutCalradicExchange) { $requiredModules += 'Bannerlord.CalradicExchange' }

$missingModules = @($requiredModules | Where-Object {
    -not (Test-Path -LiteralPath (Join-Path $GameDir "Modules\$_\SubModule.xml"))
})
if ($missingModules.Count -gt 0) {
    throw "Missing Bannerlord modules: $($missingModules -join ', ')"
}

$binaryDir = Join-Path $GameDir 'bin\Win64_Shipping_Client'
$blse = Join-Path $binaryDir 'Bannerlord.BLSE.Standalone.exe'
$vanilla = Join-Path $binaryDir 'Bannerlord.exe'
$exe = if (Test-Path -LiteralPath $blse) { $blse } else { $vanilla }
if (-not (Test-Path -LiteralPath $exe)) { throw "Bannerlord executable not found under $binaryDir" }

$moduleArg = '_MODULES_*' + ($requiredModules -join '*') + '*_MODULES_'
Write-Host "[GABS] Launching $([IO.Path]::GetFileName($exe)) with $($requiredModules.Count) modules"
Push-Location $binaryDir
try {
    & $exe /singleplayer $moduleArg
}
finally {
    Pop-Location
}
