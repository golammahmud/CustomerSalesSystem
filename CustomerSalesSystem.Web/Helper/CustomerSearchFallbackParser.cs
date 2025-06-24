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

            query = NormalizeSynonyms(query.ToLower());

            var phrases = query.Split(new[] { " and ", ",", ".", ";", " also ", " with " }, StringSplitOptions.RemoveEmptyEntries);
            bool hasMatched = false;

            foreach (var phrase in phrases)
            {
                foreach (var field in AllowedFields.Keys)
                {
                    if (!phrase.Contains(field)) continue;

                    var fieldName = AllowedFields[field];
                    var op = DetectOperator(phrase, fieldName);

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
                            hasMatched = true;
                            break;
                        }
                    }
                }
            }

            // Fallback: no match but simple text like "find oliver"
            // Fallback: no explicit field matched, try treating full query as Name search
            if (!hasMatched)
            {
                foreach (var phrase in phrases)
                {
                    var guess = phrase.Trim();
                    if (!string.IsNullOrWhiteSpace(guess))
                    {
                        result.Filters.Add(new AIFieldFilter
                        {
                            Field = "Name",
                            Operator = "Contains",
                            Value = guess
                        });

                        break; // Only add one fallback guess
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
            phrase = phrase.ToLowerInvariant();

            if (field.Equals("Email", StringComparison.OrdinalIgnoreCase) ||
                field.Equals("Phone", StringComparison.OrdinalIgnoreCase) ||
                field.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                if (phrase.Contains("equals")) return "Equals";
                if (phrase.Contains("starts with")) return "StartsWith";
                if (phrase.Contains("ends with")) return "EndsWith";

                // Treat "is" as Contains
                return "Contains";
            }

            return "Contains"; // Safe fallback
        }

    }
}
