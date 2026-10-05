// Matheus Marques Stefani
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
namespace AcademiaDoZe.Application.Tests;
public class DatabaseSettingsTests
{
 [Theory]
 [InlineData(AppDatabaseType.SqlServer)]
 [InlineData(AppDatabaseType.MySql)]
 [InlineData(AppDatabaseType.Sqlite)]
 public void EnumFazIdaEVolta(AppDatabaseType type)=>Assert.Equal(type,type.ToDatabaseType().ToAppDatabaseType());
 [Fact] public void EnumInvalidoRejeitado()=>Assert.Throws<ArgumentOutOfRangeException>(()=>((AppDatabaseType)99).ToDatabaseType());
 [Theory]
 [InlineData("")]
 [InlineData("relativo.db")]
 public void SQLiteExigeCaminhoCompleto(string path)=>Assert.Throws<ArgumentException>(()=>new DatabaseSettings(AppDatabaseType.Sqlite,path).Build());
 [Fact] public void SQLitePreservaCaminhoComPontoEVirgula()
 {
  var path=Path.Combine(Path.GetTempPath(),"teste;academia.db");
  var cs=new SqliteConnectionStringBuilder(new DatabaseSettings(AppDatabaseType.Sqlite,path,Options:"Pooling=False").Build().ConnectionString);
  Assert.Equal(path,cs.DataSource);Assert.True(cs.ForeignKeys);Assert.False(cs.Pooling);
 }
 [Theory]
 [InlineData("Password=segredo")]
 [InlineData("Pwd=segredo")]
 public void SenhaNaoPodeSerSalvaNasOpcoes(string options)=>Assert.Throws<ArgumentException>(()=>new DatabaseSettings(AppDatabaseType.Sqlite,Path.Combine(Path.GetTempPath(),"senha.db"),Options:options).Build());
 [Theory]
 [InlineData(AppDatabaseType.SqlServer)]
 [InlineData(AppDatabaseType.MySql)]
 public void ServidorExigeCampos(AppDatabaseType type)=>Assert.Throws<ArgumentException>(()=>new DatabaseSettings(type,"").Build());
 [Fact] public void SqlServerEscapaSenha()
 {
  var cs=new SqlConnectionStringBuilder(new DatabaseSettings(AppDatabaseType.SqlServer,"","localhost","academia","aluno","abc;123").Build().ConnectionString);
  Assert.Equal("abc;123",cs.Password);Assert.Equal("academia",cs.InitialCatalog);Assert.False(cs.IntegratedSecurity);
 }
 [Fact] public void MySqlEscapaSenha()
 {
  var cs=new MySqlConnectionStringBuilder(new DatabaseSettings(AppDatabaseType.MySql,"","localhost","academia","aluno","abc;123").Build().ConnectionString);
  Assert.Equal("abc;123",cs.Password);Assert.Equal("academia",cs.Database);
 }
 [Fact] public async Task TrocarSQLiteUsaOutroBancoEVoltarPreservaCadastro()
 {
  var first=Path.Combine(Path.GetTempPath(),$"academia09-{Guid.NewGuid():N}.db");
  var second=Path.Combine(Path.GetTempPath(),$"academia10-{Guid.NewGuid():N}.db");
  var config=new DatabaseSettings(AppDatabaseType.Sqlite,first,Options:"Pooling=False").Build();
  try
  {
   using var provider=new ServiceCollection().AddSingleton(config).AddApplicationServices().BuildServiceProvider();
   using(var scope=provider.CreateScope())
   {
    var service=scope.ServiceProvider.GetRequiredService<ILogradouroService>();
    var dto=await service.AdicionarAsync(new LogradouroDto{Cep="88010000",Nome="Matheus Marques Stefani",Bairro="Centro",Cidade="Florianópolis",Estado="SC",Pais="Brasil"});
    dto.Bairro="Abc Bolinhas";await service.AtualizarAsync(dto);
   }
   config.ConnectionString=new DatabaseSettings(AppDatabaseType.Sqlite,second,Options:"Pooling=False").Build().ConnectionString;
   using(var scope=provider.CreateScope())Assert.Empty(await scope.ServiceProvider.GetRequiredService<ILogradouroService>().ObterTodosAsync());
   config.ConnectionString=new DatabaseSettings(AppDatabaseType.Sqlite,first,Options:"Pooling=False").Build().ConnectionString;
   using(var scope=provider.CreateScope())
   {
    var service=scope.ServiceProvider.GetRequiredService<ILogradouroService>();
    var dto=Assert.Single(await service.ObterTodosAsync());Assert.Equal("Abc Bolinhas",dto.Bairro);
    Assert.True(await service.RemoverAsync(dto.Id));Assert.Empty(await service.ObterTodosAsync());
   }
  }
  finally {if(File.Exists(first))File.Delete(first);if(File.Exists(second))File.Delete(second);}
 }
}
