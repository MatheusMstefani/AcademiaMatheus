// Matheus Marques Stefani
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace AcademiaDoZe.Application.Security;

// Formato da DEV08: ARGON2ID:iterações:memóriaKB:paralelismo:salt:hash.
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 3;
    private const int MemorySizeKb = 64 * 1024;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        int parallelism = Math.Clamp(Environment.ProcessorCount, 1, 4);
        var hash = Derive(password, salt, Iterations, MemorySizeKb, parallelism);
        return $"ARGON2ID:{Iterations}:{MemorySizeKb}:{parallelism}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string? password, string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash)
            || passwordHash.Length > 256) return false;
        var parts = passwordHash.Split(':');
        if (parts.Length != 6 || parts[0] != "ARGON2ID"
            || !int.TryParse(parts[1], out int iterations)
            || !int.TryParse(parts[2], out int memory)
            || !int.TryParse(parts[3], out int parallelism)) return false;
        // Não executar parâmetros arbitrariamente caros vindos de um hash malformado.
        if (iterations is < 1 or > 6 || memory is < 8 or > MemorySizeKb
            || parallelism is < 1 or > 64 || memory < 8 * parallelism) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[4]);
            var expected = Convert.FromBase64String(parts[5]);
            if (salt.Length != SaltSize || expected.Length != HashSize) return false;
            var actual = Derive(password, salt, iterations, memory, parallelism);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
        catch (ArgumentException) { return false; }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int memory, int parallelism)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt, Iterations = iterations, MemorySize = memory, DegreeOfParallelism = parallelism
        };
        return argon.GetBytes(HashSize);
    }
}
