# Academia do Ze

Trabalho de Matheus Marques Stefani.

## Continuacao dev08

A camada `AcademiaDoZe.Application` implementa os servicos de Logradouro, Aluno,
Colaborador e Matricula. Possui DTOs, enums para a interface, mapeamentos,
contratos de servico, hash Argon2id e registro de injecao de dependencia.

- 235 testes de dominio, 70 de infraestrutura e 45 da aplicacao: 350 no total.
- Os novos testes usam arquivos SQLite temporarios isolados.
- Cadastro e troca de senha pelos servicos guardam hash; as consultas nunca devolvem senha ou hash.
- O suporte aos tres bancos continua na infraestrutura. A verificacao desta atividade foi feita com SQLite.
- Os servicos de acesso ficam para a atividade futura indicada no PDF, nao fazem parte da DEV08.

Para estudar a nova camada e entender como configurar os servicos, leia
[o resumo da DEV08](docs/DEV08-estudo.md).

```powershell
dotnet test AcademiaDoZe.Application.Tests
```

Entrega DEV08: uma print com todos os diretorios da camada Application expandidos
e o nome completo visivel, mais o ZIP de toda a solucao baixada do GitHub.
O link do repositorio pode acompanhar, mas nao substitui o ZIP pedido no PDF.

## Continuacoes dev05, dev06 e dev07

- dev05: infraestrutura ADO.NET, criacao automatica das tabelas e LogradouroRepository.
- dev06: AlunoRepository e ColaboradorRepository.
- dev07: MatriculaRepository.
- 235 testes de dominio e 70 testes de infraestrutura.
- Provedores SQLite, MySQL e SQL Server. Nesta entrega, a execucao foi feita somente com SQLite.

O projeto usa .NET 10, DbProviderFactory, comandos parametrizados e scripts SQL embarcados. As interfaces de acesso de alunos e colaboradores continuam no dominio para as proximas atividades; nao fazem parte das continuacoes dev05 a dev07.

## Executar

Abra a pasta que contem AcademiaDoZe.sln no VS Code. No terminal:

```powershell
dotnet restore
dotnet test
```

O SQLite e criado automaticamente em `Dados/db_academia_do_ze.db`. Essa pasta guarda somente dados ficticios dos testes e nao e enviada ao GitHub. Os testes deixam registros para os prints e podem ser executados novamente.

Para executar somente uma atividade:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Testar.ps1 -Entidade Logradouro
powershell -ExecutionPolicy Bypass -File scripts/Testar.ps1 -Entidade Aluno
powershell -ExecutionPolicy Bypass -File scripts/Testar.ps1 -Entidade Colaborador
powershell -ExecutionPolicy Bypass -File scripts/Testar.ps1 -Entidade Matricula
```

Tambem existem tarefas no menu Terminal > Run Task do VS Code.

## Outros bancos

Os mesmos testes e repositorios funcionam com os tres provedores. E preciso disponibilizar o servidor e criar antes o database `db_academia_do_ze`; a infraestrutura cria as tabelas. MySQL e SQL Server nao foram executados nesta entrega, portanto o suporte implementado ainda precisa ser validado nesses servidores.

Use um banco dedicado aos testes: eles inserem, alteram e removem registros ficticios. Nao use um banco de producao.

```powershell
$env:ACADEMIA_DB = 'MySql'
$env:ACADEMIA_CONNECTION_STRING = 'Server=localhost;Database=db_academia_do_ze;User ID=SEU_USUARIO;Password=SUA_SENHA;UseAffectedRows=False'
dotnet test AcademiaDoZe.Infrastructure.Tests
```

```powershell
$env:ACADEMIA_DB = 'SqlServer'
$env:ACADEMIA_CONNECTION_STRING = 'Server=localhost;Database=db_academia_do_ze;User ID=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=True'
dotnet test AcademiaDoZe.Infrastructure.Tests
```

`TrustServerCertificate=True` e apenas para o servidor local de desenvolvimento. Credenciais reais devem ficar em variaveis de ambiente e nunca em commits. Para voltar ao SQLite:

```powershell
Remove-Item Env:ACADEMIA_DB -ErrorAction SilentlyContinue
Remove-Item Env:ACADEMIA_CONNECTION_STRING -ErrorAction SilentlyContinue
dotnet test AcademiaDoZe.Infrastructure.Tests
```

## Dados pedidos nos testes

- Logradouro: nome `Matheus`, bairro `Marques Stefani`, cidade com o SGBD.
- Aluno e colaborador: nome `Matheus`, complemento `Marques Stefani`, senha ficticia contendo o SGBD.
- Matricula: objetivo `Matheus Marques Stefani`, obs_restricao com o SGBD.

Os testes antigos de infraestrutura usam senhas ficticias em texto conforme os exemplos das atividades anteriores.
A partir da DEV08, os cadastros e trocas feitos pelos servicos da aplicacao usam Argon2id.
Registros antigos nao sao convertidos automaticamente; trocar a senha pelo servico grava o hash.

## Prints e entrega

Abra o banco no DB Browser for SQLite e execute as consultas de `scripts/consultas-entrega.sql`, uma por vez. Capture a interface completa com o SQL e o resultado legiveis.

- dev05 (SQLite, conforme escolha): print dos testes de Logradouro e print do SELECT de tb_logradouro.
- dev06 (SQLite, conforme escolha): print dos testes de Aluno, SELECT de tb_aluno, testes de Colaborador e SELECT de tb_colaborador. O PDF original pede 12 prints, incluindo os outros dois bancos; entregar somente os quatro de SQLite depende dessa adaptacao ser aceita pelo professor.
- dev07: print dos testes de Matricula, SELECT de tb_matricula e ZIP da solucao baixada do GitHub. Os prints sao anexados separadamente do ZIP.

As pastas de entrega e os resultados locais ficam fora do repositorio. O ZIP do GitHub contem o codigo; restaurar os pacotes e executar os testes recria o banco.
