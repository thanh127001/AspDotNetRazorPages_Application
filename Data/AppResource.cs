using System.ComponentModel.DataAnnotations;

namespace Application.Data
{
    public class AppResource
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Culture { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Protected { get; set; }
    }
}
