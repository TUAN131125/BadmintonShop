namespace BadmintonShop.Core.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }

        public bool IsSystemRole { get; set; } = false;

        public ICollection<User> Users { get; set; }
    }
}
