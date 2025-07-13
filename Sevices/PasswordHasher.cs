using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Services
{
    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int Iterations = 10000;

        public (byte[] Hash, byte[] Salt) CreateHash(string password)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256);
            return (deriveBytes.GetBytes(HashSize), deriveBytes.Salt);
        }

        public bool VerifyHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(
                password,
                storedSalt,
                Iterations,
                HashAlgorithmName.SHA256);
            return CryptographicOperations.FixedTimeEquals(deriveBytes.GetBytes(HashSize), storedHash);
        }
    }
}