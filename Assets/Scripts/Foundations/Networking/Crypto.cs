using System;
using System.Security.Cryptography;
using System.Text;

namespace Networking
{
    public class Crypto
    {
        public static string MD5(string input)
        {
            using (MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string Base64Encode(string input)
        {
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytesToEncode);
        }

        public static string Base64Decode(string input)
        {
            byte[] decodedBytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(decodedBytes);
        }
    }
}
