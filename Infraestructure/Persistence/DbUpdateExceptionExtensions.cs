using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace urlshortenerapi.Infraestructure.Persistence
{
    public static class DbUpdateExceptionExtensions
    {
        // 2601: clave duplicada en índice único. 2627: violación de restricción UNIQUE o PRIMARY KEY.
        public static bool IsUniqueConstraintViolation(this DbUpdateException exception) =>
            exception.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
