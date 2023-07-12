using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO
{
    public class UserModelCreateDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password must have at least 6 characters.")]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public int MedicBranchId { get; set; }
        [Required]
        public int MedicCompanyId { get; set; }
        [Required]
        public int SalutationId { get; set; }
        [Required]
        public int GenderCategoryId { get; set; }
        public int[] UserGroupIds { get; set; }
    }


    public class UserAuthUpdatePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmNewPassword { get; set; }
    }


    public class UserAuthChangeForgottenPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmNewPassword { get; set; }
    }

}
