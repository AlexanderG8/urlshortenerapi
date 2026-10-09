using Microsoft.EntityFrameworkCore;
using urlshortenerapi.Features.Analytics;
using urlshortenerapi.Features.Links;

namespace urlshortenerapi.Infraestructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<ShortLink> ShortLinks => Set<ShortLink>();
        public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
