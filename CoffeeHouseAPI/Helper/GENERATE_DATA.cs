using System;
using System.Text;

namespace CoffeeHouseAPI.Helper
{
    public static class GENERATE_DATA
    {
        public static string GenerateNumber(int stringLength)
        {
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateString(int stringLength)
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GeneratePassword(int passwordLength) {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string specialChars = "!@#$%^&*()_-+=<>?.";

            string allChars = lowercase + uppercase + specialChars;

            StringBuilder result = new StringBuilder(passwordLength);
            Random random = new Random();

            result.Append(lowercase[random.Next(lowercase.Length)]);
            result.Append(uppercase[random.Next(uppercase.Length)]);
            result.Append(specialChars[random.Next(specialChars.Length)]);

            for (int i = 3; i < passwordLength; i++)
            {
                result.Append(allChars[random.Next(allChars.Length)]);
            }

            return ShuffleString(result.ToString());
        }

        private static string ShuffleString(string str)
        {
            Random random = new Random();
            char[] array = str.ToCharArray();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                char temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
            return new string(array);
        }
    }
}
