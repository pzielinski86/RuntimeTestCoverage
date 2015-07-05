using System.Data.Entity;

namespace Data.SqlServer
{
    public sealed class BotsDataContext : DbContext
    {
        public BotsDataContext(string connectionString)
            : base(connectionString)
        {
        }
        public DbSet<Bot> Bots { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("AspNetUsers").
                HasMany(u => u.Bots).
                WithRequired(b => b.User).
                HasForeignKey(b => b.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}