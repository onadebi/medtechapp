using MedTechAPI.AppCore.AppGlobal.Interface;
using MedTechAPI.Domain.DTO.MedicCompanyDTO;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.MedicCenter.Interface
{
    public interface IMedicBranchRepository : ICommonBaseRepo<MedicBranchRegistrationDTO, MedicBranchUpdateDTO, MedicBranchDetailsDTO>
    {
        Task<GenResponse<List<MedicBranchDetailsDTO>>> FetchAllMedicBranchesByCompanyId(int Id, bool includeInActive = false);
        #region MedicBranch
        //Task<GenResponse<int>> RegisterNewMedicBranch(MedicBranchRegistrationDTO model);
        //Task<GenResponse<bool>> UpdateMedicBranchDetails(MedicBranchUpdateDTO model);
        //Task<GenResponse<MedicBranchDetailsDTO>> FetchMedicBranchDetailsById(int id);
        #endregion
    }
}
