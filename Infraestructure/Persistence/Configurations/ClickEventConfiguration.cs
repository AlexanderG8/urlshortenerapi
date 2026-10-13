using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using urlshortenerapi.Features.Analytics;
using urlshortenerapi.Features.Links;

namespace urlshortenerapi.Infraestructure.Persistence.Configurations
{
    internal sealed class ClickEventConfiguration : IEntityTypeConfiguration<ClickEvent>
    {
        public void Configure(EntityTypeBuilder<ClickEvent> builder)
        {
            builder.ToTable("ClickEvents");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.RefererHost)
                .HasMaxLength(ClickEvent.RefererHostMaxLength);

            builder.Property(c => c.UserAgent)
                .HasMaxLength(ClickEvent.UserAgentMaxLength);

            builder.Property(c => c.IpHash)
                .HasMaxLength(ClickEvent.IpHashLength)
                .IsFixedLength(); // binary(32)

            builder.HasOne<ShortLink>()
                .WithMany()
                .HasForeignKey(c => c.ShortLinkId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para las estadísticas: "clics del enlace X entre fecha A y B".
            // Cubre todas las consultas de estadísticas sin leer la tabla.
            builder.HasIndex(c => new { c.ShortLinkId, c.OccurredAt })
                .IncludeProperties(c => new { c.RefererHost, c.IpHash });
        }
    }
}
