using System.Text.RegularExpressions;

namespace CustomerSalesSystem.Web.Helper
{
    public static class TextCorrectionHelper
    {
        private static readonly Dictionary<string, string> _corrections = new(StringComparer.OrdinalIgnoreCase)
    {
        { "cell", "sale" },
        { "sail", "sale" },
        { "sells", "sales" },
        { "costumer", "customer" },
        { "client", "customer" },
        { "pace", "page" },
        { "listpace", "list page" },
        { "edit sail", "edit sale" },
        { "edit cell", "edit sale" },
        { "create sail", "create sale" },
        { "crete", "create" },
        { "creat", "create" },
        { "home page", "home" },
        { "main page", "home" },
        { "cstomer", "customer" },
        { "custmor", "customer" },
        { "listpage", "list page" },
        { "salespage", "sales page" },
        { "customar", "customer" }
    };

        public static string AutoCorrectVoiceQuery(string input)
        {
            foreach (var (wrong, correct) in _corrections)
            {
                input = Regex.Replace(input, $@"\b{Regex.Escape(wrong)}\b", correct, RegexOptions.IgnoreCase);
            }

            return input;
        }
    }

}
