using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO.MedicCompanyDTO
{
    public class MedicBranchRegistrationDTO
    {
        [Required]
        [StringLength(500)]
        public string BranchName { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int StateId { get; set; }

        [Required]
        [StringLength(500)]
        public string BranchAddress { get; set; }
        [Required]
        public int CompanyId { get; set; }
        public IFormFile LogoImage { get; set; }
    }

    public class MedicBranchDetailsDTO
    {
        public int Id { get; set; }
        public string BranchName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string BranchAddress { get; set; }
        public string LogoPath { get; set; }
    }
    public class MedicBranchUpdateDTO: MedicBranchRegistrationDTO
    {
        [Required]
        public int Id { get; set; }
    }


}
