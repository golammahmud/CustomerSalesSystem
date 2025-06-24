using CustomerSalesSystem.Application.DTOs;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CustomerSalesSystem.Web.Helper
{
    public static class CustomerSearchFallbackParser
    {
        private static readonly Dictionary<string, string> FieldSynonyms = new()
        {
            { "gmail", "email" },
            { "mail", "email" },
            { "email address", "email" },
            { "phone number", "phone" },
            { "contact", "phone" },
            { "number", "phone" }
        };

        private static readonly Dictionary<string, string> AllowedFields = new()
        {
            { "name", "Name" },
            { "email", "Email" },
            { "phone", "Phone" }
        };

        public static AIQueryResult Parse(string query)
        {
            var result = new AIQueryResult();
            if (string.IsNullOrWhiteSpace(query)) return result;

            // Normalize field synonyms
            query = NormalizeSynonyms(query.ToLower());

            // Split phrases
            var phrases = query.Split(new[] { " and ", ",", ".", ";", " also ", " with " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var phrase in phrases)
            {
                foreach (var field in AllowedFields.Keys)
                {
                    if (!phrase.Contains(field)) continue;

                    var fieldName = AllowedFields[field];
                    var op = DetectOperator(phrase, fieldName);

                    // Regex pattern to extract value
                    var pattern = $@"{field}\s*(is|equals|starts with|ends with)?\s*(.+)";
                    var match = Regex.Match(phrase, pattern, RegexOptions.IgnoreCase);

                    if (match.Success && match.Groups.Count >= 3)
                    {
                        var rawValue = match.Groups[2].Value.Trim().Trim('"', '\'', ':');
                        if (!string.IsNullOrWhiteSpace(rawValue))
                        {
                            result.Filters.Add(new AIFieldFilter
                            {
                                Field = fieldName,
                                Operator = op,
                                Value = rawValue
                            });
                            break; // stop after match
                        }
                    }
                }
            }

            return result;
        }

        private static string NormalizeSynonyms(string input)
        {
            foreach (var kvp in FieldSynonyms)
                input = Regex.Replace(input, $@"\b{kvp.Key}\b", kvp.Value, RegexOptions.IgnoreCase);

            return input;
        }

        private static string DetectOperator(string phrase, string field)
        {
            if (field.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                // Email default is "Contains"
                if (phrase.Contains("equals")) return "Equals";
                return "Contains";
            }

            if (phrase.Contains("starts with")) return "StartsWith";
            if (phrase.Contains("ends with")) return "EndsWith";
            if (phrase.Contains("equals") || Regex.IsMatch(phrase, @"\bis\b")) return "Equals";

            return "Contains";
        }
    }
}
