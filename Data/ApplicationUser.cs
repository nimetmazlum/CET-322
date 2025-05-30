using Microsoft.AspNetCore.Identity;

namespace CET322.Data
{
    public class ApplicationUser : IdentityUser
    {
        
        public string FirstName { get; set; } = string.Empty; 
        public string LastName { get; set; } = string.Empty;  

        
        public int? CompanyId { get; set; }
        public CET322.Models.Company? Company { get; set; }
    }
}
