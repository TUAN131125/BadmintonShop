namespace BadmintonShop.Core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? Province { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsGuest { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
