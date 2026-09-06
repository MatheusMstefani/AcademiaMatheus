# Matheus Marques Stefani
param(
    [ValidateSet('Logradouro','Aluno','Colaborador','Matricula','Todas')]
    [string]$Entidade = 'Todas'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    $testArgs = @('test', 'AcademiaDoZe.Infrastructure.Tests', '--nologo', '--verbosity', 'normal', '--logger', "trx;LogFileName=$Entidade.trx")
    if ($Entidade -ne 'Todas') { $testArgs += @('--filter', "FullyQualifiedName~${Entidade}InfrastructureTests") }
    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) { throw 'Os testes falharam. Confira a mensagem acima.' }
    Write-Host "Banco SQLite: $projectRoot\Dados\db_academia_do_ze.db"
} finally { Pop-Location }
