using System.ComponentModel.DataAnnotations;

namespace Application.Data
{
    public class AppUser
    {
        public string Id { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;        
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string StatusId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;        
        public string? CreatedUserId { get; set; }
        public string? ModifiedUserId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
