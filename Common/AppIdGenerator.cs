namespace Application.Common
{
    /// <summary>Generates time-ordered IDs based on UUID version 7.</summary>
    public static class AppIdGenerator
    {
        /// <summary>Creates a UUID v7 formatted as 32 hexadecimal characters without hyphens.</summary>
        public static string GenerateId()
        {
            var id = Guid.CreateVersion7();
            return id.ToString("N");
        }
    }
}
