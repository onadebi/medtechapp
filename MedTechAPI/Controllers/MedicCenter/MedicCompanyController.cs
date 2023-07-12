using MedTechAPI.AppCore.MedicCenter.Interface;
using MedTechAPI.Controllers.Setup;
using MedTechAPI.Domain.DTO.MedicCompanyDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnaxTools.Dto.Http;

namespace MedTechAPI.Controllers.MedicCenter
{
    [AllowAnonymous]
    public class MedicCompanyController : BaseSetupController
    {
        private readonly IMedicCompanyRepository _medicCompanyRepo;
        private readonly IMedicBranchRepository _medicBranchRepo;

        public MedicCompanyController(IMedicCompanyRepository medicCompanyRepo
            , IMedicBranchRepository medicBranchRepo)
        {
            _medicCompanyRepo = medicCompanyRepo;
            _medicBranchRepo = medicBranchRepo;
        }

        #region =================MEDIC COMPNAY===============

        [HttpGet(nameof(FetchMedicCompanies))]
        [ProducesResponseType(typeof(GenResponse<List<MedicCompanyDetailsDTO>>), 200)]
        public async Task<IActionResult> FetchMedicCompanies()
        {
            return Ok(await _medicCompanyRepo.FetchAllMedicCompanyDetails());
        }

        [HttpGet(nameof(FetchMedicCompanyById))]
        [ProducesResponseType(typeof(GenResponse<MedicCompanyDetailsDTO>), 200)]
        public async Task<IActionResult> FetchMedicCompanyById(int id)
        {
            return Ok(await _medicCompanyRepo.FetchMedicCompanyDetailsById(id));
        }

        [HttpPost(nameof(RegisterNewMedicCompany))]
        [ProducesResponseType(typeof(GenResponse<int>), 200)]
        public async Task<IActionResult> RegisterNewMedicCompany([FromForm]MedicCompanyRegistrationDTO model)
        {
            var objResp = await _medicCompanyRepo.RegisterNewMedicCompany(model);
            return StatusCode(objResp.StatCode, objResp);
        }

        [HttpPut(nameof(UpdateMedicCompanyDetail))]
        [ProducesResponseType(typeof(GenResponse<bool>), 200)]
        public async Task<IActionResult> UpdateMedicCompanyDetail(MedicCompanyUpdateDTO model)
        {
            var objResp = await _medicCompanyRepo.UpdateMedicCompanyDetails(model);
            return StatusCode(objResp.StatCode, objResp);
        }

        #endregion


        #region =================Medic Branches==============
        [HttpGet(nameof(FetchMedicBranches))]
        [ProducesResponseType(typeof(GenResponse<List<MedicBranchDetailsDTO>>), 200)]
        public async Task<IActionResult> FetchMedicBranches()
        {
            return Ok(await _medicBranchRepo.FetchAll());
        }

        [HttpGet(nameof(FetchMedicBranchDetailsById))]
        [ProducesResponseType(typeof(GenResponse<MedicBranchDetailsDTO>), 200)]
        public async Task<IActionResult> FetchMedicBranchDetailsById(int id)
        {
            return Ok(await _medicBranchRepo.FetchById(id));
        }

        [HttpPost(nameof(RegisterNewMedicBranch))]
        [ProducesResponseType(typeof(GenResponse<int>), 200)]
        public async Task<IActionResult> RegisterNewMedicBranch([FromForm] MedicBranchRegistrationDTO model)
        {
            var objResp = await _medicBranchRepo.Add(model);
            return StatusCode(objResp.StatCode, objResp);
        }

        [HttpPut(nameof(UpdateMedicBranchDetails))]
        [ProducesResponseType(typeof(GenResponse<bool>), 200)]
        public async Task<IActionResult> UpdateMedicBranchDetails(MedicBranchUpdateDTO model)
        {
            var objResp = await _medicBranchRepo.Update(model);
            return StatusCode(objResp.StatCode, objResp);
        }
        #endregion

    }
}
