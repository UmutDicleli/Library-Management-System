using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyLibrary
{
    public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();

            optionsBuilder.UseSqlServer(
                "Data Source=DESKTOP-CF1BA8E;Initial Catalog=Dbnew2;Integrated Security=True;Trust Server Certificate=True"
            );

            return new MyDbContext(optionsBuilder.Options);
        }
    }
}
