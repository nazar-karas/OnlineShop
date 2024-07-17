using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class UserHelper
    {
        public static string CreateRandomSalt()
        {
            var bytes = new byte[4];
            Random rand = new Random();
            rand.NextBytes(bytes);

            var salt = Convert.ToBase64String(bytes);
            return salt;
        }

        public static string ComputeSaltedHash(string password, string passwordSalt)
        {
            byte[] bytesFromPassword = Encoding.Unicode.GetBytes(password);
            byte[] bytesFromSalt = Encoding.Unicode.GetBytes(passwordSalt);

            string hash = Convert.ToBase64String(bytesFromPassword
            .Union(bytesFromSalt).ToArray());

            return hash;
        }

        public static string GenerateConfirmationCode()
        {
            Random rand = new Random();

            int code = rand.Next(100000, 999999);

            return code.ToString();
        }
    }
}
