using System;
using System.Security.Cryptography;
using System.Text;

namespace AuthManager.Services.AuthenticationService
{
    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public static (string hash, string salt) HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            using Rfc2898DeriveBytes pbkdf2 = new(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(KeySize);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);

            using Rfc2898DeriveBytes pbkdf2 = new(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hashToVerify = pbkdf2.GetBytes(KeySize);

            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            for (int i = 0; i < KeySize; i++)
            {
                if (hashToVerify[i] != storedHashBytes[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}