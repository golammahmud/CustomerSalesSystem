using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerSalesSystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // Navigation
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
