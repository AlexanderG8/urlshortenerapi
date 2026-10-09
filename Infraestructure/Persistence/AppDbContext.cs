using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using urlshortenerapi.Features.Analytics;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Features.Links;

namespace urlshortenerapi.Infraestructure.Persistence
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
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
