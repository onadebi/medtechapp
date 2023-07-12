using MedTechAPI.Domain.DTO.MedicCompanyDTO;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.MedicCenter.Interface
{
    public interface IMedicCompanyRepository
    {
        #region MedicCompany
        Task<GenResponse<int>> RegisterNewMedicCompany(MedicCompanyRegistrationDTO model);
        Task<GenResponse<bool>> UpdateMedicCompanyDetails(MedicCompanyUpdateDTO model);
        Task<GenResponse<List<MedicCompanyDetailsDTO>>> FetchAllMedicCompanyDetails();
        Task<GenResponse<MedicCompanyDetailsDTO>> FetchMedicCompanyDetailsById(int id);
        Task<GenResponse<bool>> Remove(int Id);
        #endregion

    }
}
