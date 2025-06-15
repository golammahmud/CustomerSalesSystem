using System.Text.Json.Serialization;

namespace CustomerSalesSystem.Application.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class AIFieldFilter
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = default!;

        [JsonPropertyName("operator")]
        public string Operator { get; set; } = default!;

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
    public class AIQueryResult
    {
        [JsonPropertyName("filters")]
        public List<AIFieldFilter> Filters { get; set; } = new();
    }

    public class AIIntentResult
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = default!; // e.g. "SearchCustomer", "SearchProduct", "SearchSales"

        [JsonPropertyName("topic")]
        public string Topic { get; set; } = default!;
        [JsonPropertyName("entities")]
        public List<AIFieldFilter> Entities { get; set; } = new();
    }

    public class SearchRequestModel
    {
        public string Query { get; set; }
        public string Language { get; set; }
        public bool VoiceMode { get; set; }
    }


}
