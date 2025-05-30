﻿namespace CustomerSalesSystem.Application.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? SaleDate { get; set; }
    }
}
