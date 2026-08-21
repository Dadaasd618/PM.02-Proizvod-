using System.Linq;

namespace ClinicApp.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            string digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.Length >= 10 && digits.Length <= 11;
        }

        public static bool IsValidPolicy(string policy)
        {
            if (string.IsNullOrEmpty(policy)) return false;
            return policy.Length == 16 && policy.All(char.IsDigit);
        }

        public static bool IsValidPrice(string priceText, out decimal price)
        {
            return decimal.TryParse(priceText.Replace('.', ','), out price) && price >= 0;
        }

        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }
    }
}