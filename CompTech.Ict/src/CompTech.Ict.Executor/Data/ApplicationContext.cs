using CompTech.Ict.Executor.Models;
using Microsoft.EntityFrameworkCore;

namespace CompTech.Ict.Executor.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<ValueModel> Values { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("sample");
        }
    }
}
