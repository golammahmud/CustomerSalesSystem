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
}
