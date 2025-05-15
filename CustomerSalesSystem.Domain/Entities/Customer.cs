namespace CustomerSalesSystem.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        // Navigation
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }

}

