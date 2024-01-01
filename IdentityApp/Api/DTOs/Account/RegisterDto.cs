using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
    public class RegisterDto
    {

        [Required]
        [StringLength(15,MinimumLength = 3,ErrorMessage = "FirstName must be atleast {2}  and maximum {1} Characters")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "LastName must be atleast {2}  and maximum {1} Characters")]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Password  must be atleast {2}  and maximum  {1} Characters")]
        public string Password { get; set; }

    }
}
