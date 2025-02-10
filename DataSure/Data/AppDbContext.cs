using Microsoft.EntityFrameworkCore;

namespace DataSure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Property> Properties { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

    public class Property
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
