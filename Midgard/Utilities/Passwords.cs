using System.Security.Cryptography;
using System.Text;

namespace Midgard.Utilities
{
    public class Passwords
    {
        private static string Md5(string str)
        {
            using var md5 = MD5.Create();
            var result = "";
            var buffer = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            foreach (var t in buffer)
            {
                result += t.ToString("X2");
            }
            return result.ToLower();
        }

        private static string Sha512(string str)
        {
            using var sha512 = SHA512.Create();
            var result = "";
            var buffer = sha512.ComputeHash(Encoding.ASCII.GetBytes(str));
            foreach (var t in buffer)
            {
                result += t.ToString("X2");
            }
            return result.ToLower();
        }

        public static string Hash(string password, string salt)
        {
            return Sha512(Md5(password) + salt);
        }
    }
}