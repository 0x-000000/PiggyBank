using System;
using System.Security.Cryptography;
using System.Text;

namespace Bank.Models
{
    public class Credentials
    {
        public string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (var b in hash)
                {
                    // store hex of hash
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            var hash = HashPassword(password);
            return string.Equals(hash, storedHash, StringComparison.Ordinal);
        }
    }
}
