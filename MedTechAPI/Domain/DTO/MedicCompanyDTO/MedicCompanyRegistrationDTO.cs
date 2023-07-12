using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MedTechAPI.Domain.DTO.MedicCompanyDTO;
public class MedicCompanyRegistrationDTO
{
    [StringLength(500)]
    public string CompanyName { get; set; }
    public int CountryId { get; set; }
    public int StateId { get; set; }
    public bool IsHeadBranch { get; set; }

    [StringLength(750)]
    public string CompanyAddress { get; set; }
    public IFormFile LogoImage { get; set; }

}

public class MedicCompanyDetailsDTO
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public int CountryId { get; set; }
    public string CountryName { get; set; }
    public int StateId { get; set; }
    public string StateName { get; set; }
    public string CompanyAddress { get; set; }
    public string LogoPath { get; set; }
}

public class MedicCompanyUpdateDTO : MedicCompanyRegistrationDTO
{
    [Required]
    public int Id { get; set; }

}


