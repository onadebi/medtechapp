using MedTechAPI.Domain.DTO.LocationDTOs;
using MedTechAPI.Domain.DTO.MedicCompanyDTO;
using MedTechAPI.Domain.DTO.Navigation;
using MedTechAPI.Domain.DTO.PatientDTOs;
using MedTechAPI.Domain.DTO.Salutation;
using MedTechAPI.Domain.Entities.PatientEntitites;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Entities.SetupConfigurations;

namespace MedTechAPI.AppCore.AppGlobal.Mapper
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PatientCategory, PatientCategoryRespDTO>().ReverseMap();
            CreateMap<PatientCategory, PatientCategoryCreationDTO>().ReverseMap();
            CreateMap<PatientProfile, RegisterNewPatientDto>().ReverseMap();
            CreateMap<MedicCompanyDetail, MedicCompanyRegistrationDTO>().ReverseMap();
            CreateMap<MedicCompanyDetail, MedicCompanyUpdateDTO>().ReverseMap();

            CreateMap<StateDetail, StateCreationRequestDTO>().ReverseMap();
            CreateMap<StateDetail, UpdateStateDetailDTO>().ReverseMap();

            CreateMap<CountryDetail, CountryUpdateRequestDTO>().ReverseMap();
            CreateMap<CountryDetail, CountryCreationRequestDTO>().ReverseMap();

            CreateMap<BranchDetail, MedicBranchRegistrationDTO>().ReverseMap();
            CreateMap<BranchDetail, MedicBranchUpdateDTO>().ReverseMap();

            CreateMap<Salutation, SalutationResponseDTO>().ReverseMap();

            CreateMap<MenuController, MenuModules>().ReverseMap();
            CreateMap<MenuControllerActions, MenuActions>().ReverseMap();
            //CreateMap<CompanyUserCreationInputDto, CompanyDetail>().ReverseMap()
            //   .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom((src, dest) => dest.EmailAddress = src.Email));
            //MedicCompanyDetail->MedicCompanyRegistrationDTO
        }
    }
}
