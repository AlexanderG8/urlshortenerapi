namespace urlshortenerapi.Features.Links
{
    public static class ShortLinkConstraints
    {
        public const int CodeMaxLength = 30;
        public const int DestinationUrlMaxLength = 2048;
        public const int OwnerIdMaxLength = 450; // Longitud estándar del Id de usuario en ASP.NET Core Identity
    }
}
