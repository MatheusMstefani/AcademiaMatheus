// Matheus Marques Stefani
using AcademiaDoZe.Application.Security;

namespace AcademiaDoZe.Application.Tests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_UsaSaltAleatorioEVerificaSenha()
    {
        var hash = PasswordHasher.Hash("Senha123!");
        var outro = PasswordHasher.Hash("Senha123!");
        Assert.StartsWith("ARGON2ID:3:65536:", hash);
        Assert.NotEqual(hash, outro);
        Assert.True(PasswordHasher.Verify("Senha123!", hash));
        Assert.False(PasswordHasher.Verify("senha123!", hash));
        Assert.False(PasswordHasher.Verify("OutraSenha", hash));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("SenhaEmTexto")]
    [InlineData("ARGON2ID:3:65536:4:???::")]
    [InlineData("ARGON2ID:3:65536:4:aaaa:bbbb")]
    [InlineData("ARGON2ID:99999999:65536:4:aaaa:bbbb")]
    [InlineData("ARGON2ID:3:2147483647:4:aaaa:bbbb")]
    [InlineData("ARGON2ID:0:65536:4:aaaa:bbbb")]
    [InlineData("ARGON2ID:3:65536:0:aaaa:bbbb")]
    [InlineData("ARGON2ID:3:8:64:aaaa:bbbb")]
    public void Verify_RejeitaHashMalformadoSemLancarExcecao(string? hash) =>
        Assert.False(PasswordHasher.Verify("Senha123!", hash));

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Hash_RejeitaSenhaVazia(string senha) =>
        Assert.ThrowsAny<ArgumentException>(() => PasswordHasher.Hash(senha));
}
