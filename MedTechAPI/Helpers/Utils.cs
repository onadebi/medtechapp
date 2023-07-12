using System.Text.RegularExpressions;

namespace MedTechAPI.Helpers
{
    public static class Utils
    {
        public static string CapitaliseFirstChar(this string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return word;
            }
            string firstChar = word.ToCharArray().FirstOrDefault().ToString().ToUpper();
            string secondWordPart = word[1..].ToLower();
            return firstChar + secondWordPart;
        }

        public static string SplitCamelCase(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) { return source; }
            string[]  splitString = Regex.Split(source, @"(?<!^)(?=[A-Z])");
            string objResp = string.Join(" ", splitString);
            return objResp;
        }
    }
}
