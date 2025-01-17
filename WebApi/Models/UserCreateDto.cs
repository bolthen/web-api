using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApi.Models
{
    public class UserCreateDto : IValidatableObject
    {
        [Required]
        [RegularExpression("^[0-9\\p{L}]*$", ErrorMessage = "Login should contain only letters or digits")]
        public string Login { get; set; }
        
        [DefaultValue("John")]
        public string FirstName { get; set; }
        
        [DefaultValue("Doe")]
        public string LastName { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Login.All(char.IsLetterOrDigit))
                yield return new ValidationResult("Логин должен состоять только из символов и цифр", new[] { nameof(Login) });
        }
    }
}