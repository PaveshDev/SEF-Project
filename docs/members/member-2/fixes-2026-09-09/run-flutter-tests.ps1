$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$mobileRoot = (Join-Path $projectRoot 'mobile').Replace('\', '/')
$harnessRoot = Join-Path ([IO.Path]::GetTempPath()) ('recovery-flutter-tests-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $harnessRoot 'test') -Force | Out-Null
$manifest = @"
name: recovery_readiness_harness
environment:
  sdk: '>=3.5.4 <4.0.0'
dependencies:
  waste_to_value:
    path: '$mobileRoot'
  flutter:
    sdk: flutter
dev_dependencies:
  flutter_test:
    sdk: flutter
flutter:
  uses-material-design: true
"@
[IO.File]::WriteAllText((Join-Path $harnessRoot 'pubspec.yaml'), $manifest)
Copy-Item -LiteralPath (Join-Path $projectRoot 'mobile/test/recovery/readiness_test.dart.template') -Destination (Join-Path $harnessRoot 'test/readiness_test.dart')
Push-Location $harnessRoot
try {
  rtk proxy flutter pub get
  if ($LASTEXITCODE -ne 0) { throw 'Test dependency resolution failed.' }
  rtk proxy flutter test
  if ($LASTEXITCODE -ne 0) { throw 'Recovery Flutter tests failed.' }
} finally { Pop-Location }
Write-Output "Test harness retained at $harnessRoot. The shared mobile manifest was not changed."
