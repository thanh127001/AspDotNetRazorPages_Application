using System.ComponentModel.DataAnnotations;
using Application.Data;

namespace Application.Models
{
    public class AppSettingModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
    }
}
