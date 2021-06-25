using Microsoft.EntityFrameworkCore;
using Riveet.Prerender.Entities;

namespace Riveet.Prerender.Contexts
{
    public class WebsiteContext : DbContext
    {
        public DbSet<WebPage> Website { get; set; }

        public WebsiteContext(DbContextOptions<WebsiteContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<WebPage>(build => 
            { 
                build.HasIndex(x => x.Url); 
            });
        }
    }
}
