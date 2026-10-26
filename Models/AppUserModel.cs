using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Application.Models
{
    public class AppUserModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;        
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int Status { get; set; }
        public int Role { get; set; }        
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
