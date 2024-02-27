using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace FleszynChat.Scripts
{
    public class Hasher
    {
        public static (string hashedPassword, string salt) HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Array.Copy(passwordBytes, saltedPassword, passwordBytes.Length);
                Array.Copy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                byte[] hashedPassword = sha256.ComputeHash(saltedPassword);

                // Convert the hashed password and salt to Base64 strings for storage
                string hashedPasswordBase64 = Convert.ToBase64String(hashedPassword);
                string saltBase64 = Convert.ToBase64String(salt);

                return (hashedPasswordBase64, saltBase64);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword, byte[] saltBytes)
        {
            // Convert the hashed password and salt from Base64 strings to byte arrays
            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Hash the entered password with the stored salt
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + saltBytes.Length];
                Array.Copy(passwordBytes, saltedPassword, passwordBytes.Length);
                Array.Copy(saltBytes, 0, saltedPassword, passwordBytes.Length, saltBytes.Length);

                byte[] hashedPasswordToVerify = sha256.ComputeHash(saltedPassword);

                // Compare the computed hash with the stored hash
                return StructuralComparisons.StructuralEqualityComparer.Equals(hashedPasswordBytes, hashedPasswordToVerify);
            }
        }
    }
}
