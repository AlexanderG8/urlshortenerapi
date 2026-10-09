using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using urlshortenerapi.Features.Links;

namespace urlshortenerapi.Infraestructure.Persistence.Configurations
{
    internal sealed class ShortLinkConfiguration : IEntityTypeConfiguration<ShortLink>
    {
        public void Configure(EntityTypeBuilder<ShortLink> builder)
        {
            builder.ToTable("ShortLinks");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Code)
                .HasMaxLength(ShortLinkConstraints.CodeMaxLength)
                .IsUnicode(false)                        // varchar: Base62 y alias son ASCII
                .UseCollation("Latin1_General_100_BIN2") // distingue mayúsculas
                .IsRequired();

            builder.Property(l => l.DestinationUrl)
                .HasMaxLength(ShortLinkConstraints.DestinationUrlMaxLength) // nvarchar(2048)
                .IsRequired();

            builder.Property(l => l.OwnerId)
                .HasMaxLength(ShortLinkConstraints.OwnerIdMaxLength)
                .IsRequired();

            builder.Property(l => l.IsActive)
                .HasDefaultValue(true);

            // Búsqueda en cada redirección: debe ser única y rápida.
            builder.HasIndex(l => l.Code)
                .IsUnique();

            // Listado paginado de "mis enlaces", ordenado por fecha.
            builder.HasIndex(l => new { l.OwnerId, l.CreatedAt });
        }
    }
}
