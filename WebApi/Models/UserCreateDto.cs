using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApi.Models
{
    public class UserCreateDto : IValidatableObject
    {
        [Required]
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Login.All(char.IsLetterOrDigit))
                yield return new ValidationResult("Логин должен состоять только из символов и цифр", new[] { nameof(Login) });
        }
    }
}