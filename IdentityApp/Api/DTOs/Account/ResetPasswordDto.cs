using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Password  must be atleast {2}  and maximum  {1} Characters")]
        public string NewPassword { get; set; }
    }
}
