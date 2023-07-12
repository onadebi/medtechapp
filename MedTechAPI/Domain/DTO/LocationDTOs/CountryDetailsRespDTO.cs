using MedTechAPI.Domain.Entities.SetupConfigurations;

namespace MedTechAPI.Domain.DTO.LocationDTOs;
public class CountryDetailsRespDTO
{
    public int Id { get; set; }

    public string CountryCode { get; set; }

    public string CountryName { get; set; }

    //public ICollection<StateDetail> StateDetails { get; set; }
}
public class CountryCreationRequestDTO
{

    public string CountryCode { get; set; }

    public string CountryName { get; set; }
}

public class CountryUpdateRequestDTO : CountryCreationRequestDTO
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
}
