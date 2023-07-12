using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO.PatientDTOs
{
    public class RegisterNewPatientDto
    {
        [Required]
        public int SalutationId { get; set; }

        [Required]
        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(250)]
        public string LastName { get; set; }

        public DateTime Dob { get; set; }

        [StringLength(100)]
        public string PatientCode { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int StateId { get; set; }

        [StringLength(1500)]
        public string Address { get; set; }

        [Required]
        public int MedicBranchId { get; set; }
    }
}