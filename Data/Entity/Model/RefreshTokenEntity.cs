namespace notion_clone.Data.Entity.Model

{
    public class RefreshTokenEntity : IEntity
    {
        public Guid Id { get; set; }
        public ApplicationUser? User { get; set; }
        public string? Token { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpireAt { get; set; }
    }
}
