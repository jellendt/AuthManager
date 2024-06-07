using System;
using System.Security.Cryptography;
using System.Text;

namespace AuthManager.Services.AuthenticationService
{
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000; // Number of iterations

        public static (string hash, string salt) HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            // Hash the password with the salt
            using Rfc2898DeriveBytes pbkdf2 = new(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(KeySize);

            // Convert the salt and hash to Base64 strings
            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            // Convert the stored salt from Base64 string to byte array
            byte[] salt = Convert.FromBase64String(storedSalt);

            // Hash the input password with the stored salt
            using Rfc2898DeriveBytes pbkdf2 = new(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hashToVerify = pbkdf2.GetBytes(KeySize);

            // Convert the stored hash from Base64 string to byte array
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            // Compare the input hash with the stored hash
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