using MedTechAPI.AppCore.ProfileManagement.Repository;
using MedTechAPI.Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnaxTools.Dto.Http;

namespace MedTechAPI.Controllers.Setup
{
    public class GenderCategoryController: BaseSetupController
    {
        private readonly IGenderCategoryRespository _genderRepo;

        public GenderCategoryController(IGenderCategoryRespository genderRepo)
        {
            _genderRepo = genderRepo;
        }

        [HttpGet(nameof(FetchAllGenderCategories))]
        [ProducesResponseType(typeof(GenResponse<List<GenderCategoryRespDTO>>), 200)]
        public async Task<IActionResult> FetchAllGenderCategories()
        {
            return Ok(await _genderRepo.FetchAll()); //FetchAllGenderCategories());
        }
    }
}
