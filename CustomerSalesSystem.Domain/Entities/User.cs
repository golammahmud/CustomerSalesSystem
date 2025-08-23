using System.ComponentModel.DataAnnotations;

namespace CustomerSalesSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Username cannot exceed 200 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(500, ErrorMessage = "Password hash cannot exceed 500 characters.")]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Refresh token hash cannot exceed 500 characters.")]
        public string RefreshTokenHash { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
