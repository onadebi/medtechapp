using MedTechAPI.AppCore.ProfileManagement.Repository;
using MedTechAPI.Domain.DTO.Salutation;
using MedTechAPI.Domain.Entities.ProfileManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnaxTools.Dto.Http;

namespace MedTechAPI.Controllers.Setup
{
    public class SalutationController : BaseSetupController
    {
        private readonly ISalutationRepository _salutationRepo;
        public SalutationController(ISalutationRepository salutationRepo)
        {
            _salutationRepo = salutationRepo;
        }

        [HttpGet(nameof(FetchAllSalutations))]
        [ProducesResponseType(typeof(GenResponse<List<SalutationResponseDTO>>), 200)]
        public async Task<IActionResult> FetchAllSalutations()
        {
            return Ok(await _salutationRepo.FetchAllSalutations());
        }
    }
}
