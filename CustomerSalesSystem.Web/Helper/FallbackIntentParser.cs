using CustomerSalesSystem.Application.DTOs;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CustomerSalesSystem.Web.Helper
{
    public static class FallbackIntentParser
    {
        private static readonly Dictionary<string, (string Table, string Type)> SearchableFields = new()
    {
        // Customer
        { "name", ("Customer", "string") },
        { "email", ("Customer", "string") },
        { "phone", ("Customer", "string") },

        // Product
        { "product", ("Product", "string") },
        { "price", ("Product", "number") },

        // Sales
        { "id", ("Sales", "number") },
        { "quantity", ("Sales", "number") },
        { "totalprice", ("Sales", "number") },
        { "saledate", ("Sales", "date") },
        { "customername", ("Sales", "string") },
        { "productname", ("Sales", "string") }
    };

        private static readonly Dictionary<string, string> NavigationTargets = new(StringComparer.OrdinalIgnoreCase)
    {
        { "customer list", "Customer List" },
        { "sales list", "Sales List" },
        { "home", "Home" },
        { "search", "Search" },
        { "create customer", "CreateCustomer" },
        { "edit customer", "EditCustomer" },
        { "create sale", "CreateSale" },
        { "edit sale", "EditSale" }
    };

        private static readonly string[] NavigationVerbs =
            ["go to", "navigate to", "open", "take me to", "show me", "edit", "create", "add", "start", "change", "update", "redirect", "move to"];

        private static readonly string[] AppUsagePhrases =
            ["what is this app", "how does this app work", "what can i do", "what is this software", "what does this do"];

        private static readonly string[] BusinessInfoPhrases =
            ["who made this", "who built this", "who developed this", "who created this app", "developer name"];

        private static readonly string[] ChitChatPhrases =
            ["hi", "hello", "how are you", "tell me a joke", "what's up"];

        public static AIIntentResult Parse(string query)
        {
            query = NormalizeFieldSynonyms(query.ToLower()).Trim();

            //query = query.ToLowerInvariant().Trim();

            // 1. Navigation
            if (NavigationVerbs.Any(verb => query.Contains(verb)))
            {
                var navTarget = NavigationTargets.Keys.FirstOrDefault(k => query.Contains(k));
                if (navTarget != null)
                {
                    var result = new AIIntentResult
                    {
                        Intent = "navigate",
                        Entities = new List<AIFieldFilter>
                    {
                        new() { Field = "target", Operator = "Equals", Value = NavigationTargets[navTarget] }
                    }
                    };

                    var idMatch = Regex.Match(query, @"\b\d+\b");
                    if (query.Contains("edit") && idMatch.Success)
                    {
                        result.Entities.Add(new()
                        {
                            Field = "id",
                            Operator = "Equals",
                            Value = idMatch.Value
                        });
                    }

                    return result;
                }
            }

            // 2. Chat
            if (AppUsagePhrases.Any(p => query.Contains(p)))
            {
                return new AIIntentResult { Intent = "Chat", Topic = "AppFeatures", Entities = [] };
            }

            if (BusinessInfoPhrases.Any(p => query.Contains(p)))
            {
                return new AIIntentResult { Intent = "Chat", Topic = "BusinessInfo", Entities = [] };
            }

            if (ChitChatPhrases.Any(p => query.Contains(p)))
            {
                return new AIIntentResult { Intent = "Chat", Topic = "ChitChat", Entities = [] };
            }

            // 3. Search
            var intent = GetSearchIntent(query);
            if (intent.StartsWith("Search"))
            {
                var entities = ExtractEntities(query, SearchableFields);
                return new AIIntentResult
                {
                    Intent = intent,
                    Entities = entities
                };
            }

            // 4. Fallback
            return new AIIntentResult
            {
                Intent = "Unknown",
                Entities = []
            };
        }

        private static string GetSearchIntent(string query)
        {
            if (query.Contains("customer")) return "SearchCustomer";
            if (query.Contains("product")) return "SearchProduct";
            if (query.Contains("sale") || query.Contains("sales")) return "SearchSales";
            return "Unknown";
        }

        private static string DetectOperator(string phrase, string fieldType)
        {
            phrase = phrase.ToLowerInvariant();

            if (fieldType == "string")
            {
                if (phrase.Contains("starts with")) return "StartsWith";
                if (phrase.Contains("ends with")) return "EndsWith";
                if (phrase.Contains("equals")) return "Equals";
                if (phrase.Contains(" is ")) return "Equals"; // cautious match
                return "Contains"; // default fallback
            }

            if (fieldType == "number" || fieldType == "date")
            {
                if (phrase.Contains("greater than") || phrase.Contains("more than")) return "GreaterThan";
                if (phrase.Contains("less than") || phrase.Contains("under")) return "LessThan";
                if (phrase.Contains("equals") || phrase.Contains(" is ")) return "Equals";
                return "Equals"; // default fallback for number/date
            }

            return "Equals";
        }


        private static List<AIFieldFilter> ExtractEntities(string query, Dictionary<string, (string Table, string Type)> fields)
        {
            var entities = new List<AIFieldFilter>();
            query = query.ToLowerInvariant();

            // Split the query into clauses (by 'and', ',', etc.)
            var phrases = query.Split(new[] { " and ", ",", ".", ";" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var phrase in phrases)
            {
                var cleanedPhrase = phrase.Trim();

                foreach (var field in fields)
                {
                    if (!cleanedPhrase.Contains(field.Key)) continue;

                    var fieldType = field.Value.Type;
                    var operatorType = DetectOperator(cleanedPhrase, fieldType);

                    // Regex pattern to find value after key (e.g., "name is lisa", "email equals test@gmail.com")
                    var pattern = $"{field.Key}\\s*(is|equals|starts with|ends with|greater than|less than)?\\s*(.+)";
                    var match = Regex.Match(cleanedPhrase, pattern);

                    if (match.Success && match.Groups.Count >= 3)
                    {
                        var rawValue = match.Groups[2].Value.Trim();

                        if (!string.IsNullOrWhiteSpace(rawValue))
                        {
                            entities.Add(new AIFieldFilter
                            {
                                Field = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(field.Key),
                                Operator = operatorType,
                                Value = rawValue
                            });

                            break; // Found one valid field match in this phrase, skip to next phrase
                        }
                    }
                }
            }

            return entities;
        }
        private static readonly Dictionary<string, string> FieldSynonyms = new()
{
    // Customer
    { "gmail", "email" },
    { "mail", "email" },
    { "contact", "phone" },
    { "phone number", "phone" },

    // Product
    { "cost", "price" },
    { "amount", "price" },

    // Sales
    { "total", "totalprice" },
    { "date", "saledate" },
    { "customer", "customername" },
    { "product", "productname" },
    { "quantity sold", "quantity" }
};
        private static string NormalizeFieldSynonyms(string query)
        {
            foreach (var synonym in FieldSynonyms)
            {
                query = query.Replace(synonym.Key, synonym.Value, StringComparison.OrdinalIgnoreCase);
            }
            return query;
        }


    }

    //public static class FallbackIntentParser
    //{
    //    private static readonly Dictionary<string, (string Table, string Type)> SearchableFields = new()
    //{
    //    // Customer
    //    { "name", ("Customer", "string") },
    //    { "email", ("Customer", "string") },
    //    { "phone", ("Customer", "string") },

    //    // Product
    //    { "product", ("Product", "string") },
    //    { "price", ("Product", "number") },

    //    // Sales
    //    { "id", ("Sales", "number") },
    //    { "quantity", ("Sales", "number") },
    //    { "totalprice", ("Sales", "number") },
    //    { "saledate", ("Sales", "date") },
    //    { "customername", ("Sales", "string") },
    //    { "productname", ("Sales", "string") }
    //};

    //    public static AIIntentResult Parse(string query)
    //    {
    //        var intent = GetSearchIntent(query);
    //        if (intent == "Unknown")
    //            return new AIIntentResult { Intent = "Unknown", Entities = new List<AIFieldFilter>() };

    //        var entities = ExtractEntities(query, SearchableFields);
    //        return new AIIntentResult
    //        {
    //            Intent = intent,
    //            Entities = entities
    //        };
    //    }

    //    private static string GetSearchIntent(string query)
    //    {
    //        query = query.ToLowerInvariant();
    //        if (query.Contains("customer")) return "SearchCustomer";
    //        if (query.Contains("product")) return "SearchProduct";
    //        if (query.Contains("sale") || query.Contains("sales")) return "SearchSales";
    //        return "Unknown";
    //    }

    //    private static string DetectOperator(string phrase, string fieldType)
    //    {
    //        phrase = phrase.ToLower();
    //        if (fieldType == "string")
    //        {
    //            if (phrase.Contains("starts with")) return "StartsWith";
    //            if (phrase.Contains("ends with")) return "EndsWith";
    //            if (phrase.Contains("equals") || phrase.Contains("is")) return "Equals";
    //            return "Contains";
    //        }

    //        if (fieldType == "number" || fieldType == "date")
    //        {
    //            if (phrase.Contains("greater than") || phrase.Contains("more than")) return "GreaterThan";
    //            if (phrase.Contains("less than") || phrase.Contains("under")) return "LessThan";
    //            if (phrase.Contains("equals") || phrase.Contains("is")) return "Equals";
    //            return "Equals";
    //        }

    //        return "Equals";
    //    }

    //    private static List<AIFieldFilter> ExtractEntities(string query, Dictionary<string, (string Table, string Type)> fields)
    //    {
    //        var entities = new List<AIFieldFilter>();
    //        query = query.ToLower();

    //        foreach (var field in fields)
    //        {
    //            if (!query.Contains(field.Key)) continue;

    //            var fieldType = field.Value.Type;
    //            var operatorType = DetectOperator(query, fieldType);

    //            var valueStart = query.IndexOf(field.Key) + field.Key.Length;
    //            var value = query.Substring(valueStart).Split(new[] { " and ", ",", ".", "with", "of" }, StringSplitOptions.RemoveEmptyEntries)[0];

    //            value = value.Replace("is", "").Replace("equals", "").Replace("starts with", "")
    //                         .Replace("ends with", "").Replace("greater than", "")
    //                         .Replace("less than", "").Replace(":", "").Trim();

    //            if (!string.IsNullOrWhiteSpace(value))
    //            {
    //                entities.Add(new AIFieldFilter
    //                {
    //                    Field = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(field.Key),
    //                    Operator = operatorType,
    //                    Value = value
    //                });
    //            }
    //        }

    //        return entities;
    //    }
    //}

}
