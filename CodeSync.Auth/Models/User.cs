using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSync.Auth.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(30, MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 30 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password hash is required")]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [RegularExpression("DEVELOPER|ADMIN",
            ErrorMessage = "Role must be DEVELOPER or ADMIN")]
        public string Role { get; set; } = "DEVELOPER";

        [StringLength(500, ErrorMessage = "Avatar URL too long")]
        public string AvatarUrl { get; set; } = string.Empty;

        [Required]
        [RegularExpression("LOCAL|GITHUB|GOOGLE",
            ErrorMessage = "Provider must be LOCAL, GITHUB or GOOGLE")]
        public string Provider { get; set; } = "LOCAL";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(300, ErrorMessage = "Bio cannot exceed 300 characters")]
        public string Bio { get; set; } = string.Empty;

        public override string ToString() =>
            $"User[{UserId}] {Username} <{Email}> Role={Role} Provider={Provider}";
    }
}