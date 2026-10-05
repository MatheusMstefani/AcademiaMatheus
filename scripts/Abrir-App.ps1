# Matheus Marques Stefani
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    dotnet run --project AcademiaDoZe.Presentation.AppMaui -f net10.0-windows10.0.19041.0
    if ($LASTEXITCODE -ne 0) { throw 'Não foi possível iniciar o aplicativo. Confira os erros acima.' }
}
finally { Pop-Location }
