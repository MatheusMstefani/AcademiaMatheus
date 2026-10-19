# Academia Matheus

Sistema de gestão de academia desenvolvido em C# com .NET 10 e interface .NET MAUI para Windows.

## Funcionalidades

- Painel com logo e totais de logradouros, alunos, colaboradores e matrículas.
- Cadastro, consulta, filtros, edição e exclusão de logradouros.
- Cadastro, listagem, busca por CPF, edição e exclusão de alunos e colaboradores.
- Seleção de foto pela galeria ou câmera, conforme disponibilidade do dispositivo.
- Endereço vinculado a um logradouro cadastrado, com busca por CEP.
- Datas de nascimento e admissão, tipo de colaborador e vínculo empregatício.
- Validação dos campos e confirmação de exclusão.
- Temas claro, escuro e sistema, com preferências salvas.
- Configuração de SQLite, MySQL e SQL Server.
- Ícones, título e layout adaptado à largura da janela.

## Estrutura

- **Domain:** entidades, objetos de valor e regras de negócio.
- **Infrastructure:** repositórios ADO.NET, comandos parametrizados e scripts de criação das tabelas.
- **Application:** serviços, DTOs, mapeamentos, validações e injeção de dependência.
- **Presentation.AppMaui:** telas, ViewModels, conversores e configurações.
- **Tests:** testes de domínio, infraestrutura e aplicação.

Os serviços de cadastro e troca de senha usam Argon2id. As consultas não retornam
senha nem hash. As senhas de conexão dos servidores são guardadas em SecureStorage.

## Executar

Requisitos: Windows 10 1809 ou superior, SDK .NET 10 e workload MAUI para Windows.

```powershell
dotnet workload install maui-windows
dotnet restore
dotnet run --project AcademiaDoZe.Presentation.AppMaui -f net10.0-windows10.0.19041.0
```

O aplicativo cria o banco SQLite inicial em AppData. O caminho pode ser consultado
e alterado na tela Configurações. As tabelas são criadas automaticamente.

## Testes

```powershell
dotnet test AcademiaDoZe.Domain.Tests
dotnet test AcademiaDoZe.Infrastructure.Tests
dotnet test AcademiaDoZe.Application.Tests
```

371 testes aprovados: 235 de domínio, 70 de infraestrutura e 66 da aplicação.
A compilação e os testes foram executados no Windows com SQLite. O suporte a
MySQL e SQL Server está implementado; a execução nesses servidores ainda não foi validada.
