using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerSalesSystem.Domain.Entities;

public class Sale
{
	public int Id { get; set; }

	[Required]
	public int CustomerId { get; set; }

	[Required]
	public int ProductId { get; set; }

	[Required]
	public int Quantity { get; set; }

	[Required, Column(TypeName = "decimal(18,2)")]
	public decimal TotalPrice { get; set; }

	[Required]
	public DateTime SaleDate { get; set; }

	// Navigation properties
	public Customer Customer { get; set; } = null!;
	public Product Product { get; set; } = null!;
}
