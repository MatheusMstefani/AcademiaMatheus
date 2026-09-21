# Matheus Marques Stefani
# Executar no terminal do VS Code, na conta normal do Windows.
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$deliveryDirectory = Join-Path (Split-Path -Parent $projectRoot) 'EntregaAcademia-DEV08'

function Invoke-GitChecked {
    param([string[]]$GitArguments)
    & git @GitArguments
    if ($LASTEXITCODE -ne 0) { throw "Git falhou: $($GitArguments -join ' ')" }
}

Push-Location $projectRoot
try {
    $branch = (& git branch --show-current).Trim()
    if ($LASTEXITCODE -ne 0 -or $branch -ne 'main') {
        throw 'Abra a branch main antes de continuar. Nenhuma troca automatica foi feita.'
    }
    $remote = (& git remote get-url origin).Trim()
    if ($LASTEXITCODE -ne 0 -or $remote -notmatch '^https://github\.com/MatheusMstefani/AcademiaMatheus(\.git)?$') {
        throw 'O repositorio remoto nao e o AcademiaMatheus esperado. Confira antes de publicar.'
    }
    & git diff --cached --quiet
    if ($LASTEXITCODE -ne 0) {
        throw 'Ja existem arquivos preparados para commit. Revise-os antes de executar este script.'
    }
    Invoke-GitChecked -GitArguments @('fetch', 'origin', 'main')
    $behind = & git rev-list --count HEAD..origin/main
    if ($LASTEXITCODE -ne 0 -or [int]$behind -gt 0) {
        throw 'Existem commits novos no GitHub. Pare e confira antes de integrar as alteracoes.'
    }

    # Inclui somente a DEV08. Nao inclui o arquivo pessoal NormalizadoService.cs.
    $files = @(
        'AcademiaDoZe.Application', 'AcademiaDoZe.Application.Tests',
        'AcademiaDoZe.sln', '.vscode/tasks.json', 'README.md',
        'docs/DEV08-estudo.md', 'scripts/Entregar-DEV08.ps1'
    )
    Invoke-GitChecked -GitArguments (@('add', '--') + $files)
    & git diff --cached --quiet
    $diffExit = $LASTEXITCODE
    if ($diffExit -eq 1) {
        Invoke-GitChecked -GitArguments @('commit', '-m', 'adicionando a camada de aplicacao da atividade 8')
    } elseif ($diffExit -ne 0) {
        throw 'Nao foi possivel conferir as alteracoes.'
    }
    Invoke-GitChecked -GitArguments @('push', 'origin', 'HEAD:main')
    $commit = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $commit -notmatch '^[0-9a-f]{40}$') {
        throw 'Nao foi possivel identificar o commit publicado.'
    }

    New-Item -ItemType Directory -Force -Path $deliveryDirectory | Out-Null
    $zipPath = Join-Path $deliveryDirectory "AcademiaMatheus-DEV08-$($commit.Substring(0, 7)).zip"
    if (Test-Path -LiteralPath $zipPath) {
        Write-Host "O ZIP deste commit ja existe: $zipPath"
    } else {
        $downloadPath = Join-Path $deliveryDirectory "download-$([guid]::NewGuid().ToString('N')).tmp"
        $url = "https://codeload.github.com/MatheusMstefani/AcademiaMatheus/zip/$commit"
        Invoke-WebRequest -Uri $url -OutFile $downloadPath -UseBasicParsing
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $archive = [System.IO.Compression.ZipFile]::OpenRead($downloadPath)
        try {
            $paths = @($archive.Entries | ForEach-Object { $_.FullName })
            if (-not ($paths -match '/AcademiaDoZe.sln$') -or
                -not ($paths -match '/AcademiaDoZe.Application/Services/MatriculaService.cs$') -or
                -not ($paths -match '/AcademiaDoZe.Application.Tests/AcademiaDoZe.Application.Tests.csproj$')) {
                throw 'O ZIP nao contem a solucao DEV08 esperada. Nao envie esse download.'
            }
        } finally { $archive.Dispose() }
        Move-Item -LiteralPath $downloadPath -Destination $zipPath
        Write-Host "ZIP baixado do GitHub e conferido: $zipPath"
    }
    Write-Host 'Falta adicionar a print 01 dev08.png nesta mesma pasta.'
    Write-Host 'Anexe o ZIP e a print na atividade do professor.'
} finally {
    Pop-Location
}
