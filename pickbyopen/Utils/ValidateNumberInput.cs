using System.Text.RegularExpressions;

namespace Pickbyopen.Utils
{
    public static class ValidateNumberInput
    {
        public static bool IsValideInput(string input)
        {
            Regex regex = new("[^0-9]+");
            return !regex.IsMatch(input);
        }
    }
}
