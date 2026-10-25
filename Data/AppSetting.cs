using System.ComponentModel.DataAnnotations;

namespace Application.Data
{
    public class AppSetting
    {
        public string Id { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
    }
}
