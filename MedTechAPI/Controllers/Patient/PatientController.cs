using MedTechAPI.AppCore.PatientService.Interface;
using MedTechAPI.Controllers.Setup;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.DTO.PatientDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnaxTools.Dto.Http;

namespace MedTechAPI.Controllers.Patient
{
    public class PatientController : BaseSetupController
    {
        private readonly IPatientRespository _patientRepo;

        public PatientController(IPatientRespository patientRepo)
        {
            _patientRepo = patientRepo;
        }

        [HttpGet(nameof(FetchPatientCategories))]
        [ProducesResponseType(typeof(GenResponse<List<PatientCategoryRespDTO>>), 200)]
        public async Task<IActionResult> FetchPatientCategories()
        {
            return Ok(await _patientRepo.FetchAllPatientCategories());
        }

        [HttpPost(nameof(AddNewPatientCategory))]
        [ProducesResponseType(typeof(GenResponse<int>), 200)]
        public async Task<IActionResult> AddNewPatientCategory(PatientCategoryCreationDTO model)
        {
            return Ok(await _patientRepo.AddNewPatientCategory(model));
        }

        [HttpPut(nameof(UpdatePatientCategory))]
        [ProducesResponseType(typeof(GenResponse<bool>), 200)]
        public async Task<IActionResult> UpdatePatientCategory(PatientCategoryUpdateDTO model)
        {
            return Ok(await _patientRepo.UpdatePatientCategory(model));
        }

        [HttpPost(nameof(AddPatient))]
        [ProducesResponseType(typeof(GenResponse<string>), 200)]
        public async Task<IActionResult> AddPatient(RegisterNewPatientDto model)
        {
            var objResp = await _patientRepo.RegisterNewPatient(model);
            return StatusCode(objResp.StatCode, objResp);
        }

        [HttpPost(nameof(BookAppointment))]
        [ProducesResponseType(typeof(GenResponse<int>), 200)]
        public async Task<IActionResult> BookAppointment(DateTime appointmentDate, object patientDetails)
        {
            throw new NotImplementedException();
            return Ok();
        }


        [HttpGet(nameof(GetAllPatientsByBranch))]
        [ProducesResponseType(typeof(GenResponse<List<AppUser>>), 200)]
        public async Task<IActionResult> GetAllPatientsByBranch(PatientCategoryCreationDTO model)
        {
            throw new NotImplementedException();
            return Ok();
        }
    }
}
