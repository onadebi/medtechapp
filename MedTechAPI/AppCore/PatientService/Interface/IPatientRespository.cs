using MedTechAPI.Domain.DTO.PatientDTOs;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.PatientService.Interface
{
    public interface IPatientRespository
    {
        Task<GenResponse<List<PatientCategoryRespDTO>>> FetchAllPatientCategories();
        Task<GenResponse<bool>> UpdatePatientCategory(PatientCategoryUpdateDTO model);
        Task<GenResponse<int>> AddNewPatientCategory(PatientCategoryCreationDTO model);
        Task<GenResponse<string>> RegisterNewPatient(RegisterNewPatientDto model);
    }
}
