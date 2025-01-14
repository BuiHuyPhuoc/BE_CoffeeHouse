using CoffeeHouseAPI.Enums;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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

        private static String CreateData(bool isSuccess)
        {
            return (isSuccess) ? "Create data success" : "Create data failed";
        }

        private static String GetData(bool isSuccess)
        {
            return (isSuccess) ? "Get data success" : "There is no item to show";
        }

        private static String DeleteData(bool isSuccess) {
            return (isSuccess) ? "Delete data success" : "Delete data failed";
        }

        private static String UpdateData(bool isSuccess)
        {
            return (isSuccess) ? "Update data success" : "Update data failed";
        }

        public static String API_ACTION_RESPONSE(bool isSuccess, Enum apiAction) {
            switch (apiAction) {
                case API_ACTION.GET:
                    return GetData(isSuccess);
                case API_ACTION.POST:
                    return CreateData(isSuccess);
                case API_ACTION.PUT:
                    return UpdateData(isSuccess);
                case API_ACTION.DELETE:
                    return DeleteData(isSuccess);
                default:
                    return string.Empty;
            }
        }

    }
}
