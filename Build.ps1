param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build'))
$ErrorActionPreference = 'Stop'
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$compiler = Join-Path $framework 'csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'Windows x64 .NET Framework compiler is required.' }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$sources = @('Gosterge.cs','Kota.cs','Yerlesim.cs','Dil.cs','AssemblyInfo.cs') | ForEach-Object { Join-Path $PSScriptRoot "Source\$_" }
$references = @('System.Windows.Forms.dll','System.Drawing.dll','System.Web.Extensions.dll',
 (Join-Path $framework 'WPF\UIAutomationClient.dll'), (Join-Path $framework 'WPF\UIAutomationTypes.dll'), (Join-Path $framework 'WPF\WindowsBase.dll'))
$arguments = @('/nologo','/target:winexe','/platform:x64','/optimize+',('/out:' + (Join-Path $OutputDirectory 'KullanimGostergesi.exe')))
$arguments += $references | ForEach-Object { '/reference:' + $_ }
& $compiler @arguments @sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Write-Output 'BUILD ONLY: unsigned EXE created; not executed, installed, signed or published.'

