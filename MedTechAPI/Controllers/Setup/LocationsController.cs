using MedTechAPI.AppCore.ProfileManagement.Repository;
using MedTechAPI.Domain.DTO.LocationDTOs;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnaxTools.Dto.Http;

namespace MedTechAPI.Controllers.Setup
{
    public class LocationsController : BaseSetupController
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IStateLocationRepository _stateLocationRepo;

        public LocationsController(ICountryRepository countryRepository
            , IStateLocationRepository stateLocationRepo)
        {
            _countryRepository = countryRepository;
            _stateLocationRepo = stateLocationRepo;
        }

        #region Country Section
        [HttpGet(nameof(FetchAllCountries))]
        [ProducesResponseType(typeof(GenResponse<List<CountryDetailsRespDTO>>), 200)]
        public async Task<IActionResult> FetchAllCountries()
        {
            return Ok(await _countryRepository.FetchAllCountries());
        }

        [HttpGet(nameof(AdminFetchAllCountries) + "/{includeDeletedAndInActive}")]
        [ProducesResponseType(typeof(GenResponse<List<CountryDetailsRespDTO>>), 200)]
        public async Task<IActionResult> AdminFetchAllCountries(bool includeDeletedAndInActive = true)
        {
            return Ok(await _countryRepository.FetchAllCountries(includeDeletedAndInActive));
        }

        [HttpGet(nameof(FetchCountryById))]
        [ProducesResponseType(typeof(GenResponse<CountryDetailsRespDTO>), 200)]
        public async Task<IActionResult> FetchCountryById(int Id)
        {
            return Ok(await _countryRepository.FetchCountryDetailById(Id));
        }

        [HttpPost(nameof(AddNewCountryLocation))]
        [ProducesResponseType(typeof(GenResponse<int>), 200)]
        public async Task<IActionResult> AddNewCountryLocation(CountryCreationRequestDTO model)
        {
            return Ok(await _countryRepository.AddNewCountryLocation(model));
        }

        [HttpPut(nameof(UpdateCountryLocation))]
        [ProducesResponseType(typeof(GenResponse<bool>), 200)]
        public async Task<IActionResult> UpdateCountryLocation(CountryUpdateRequestDTO model)
        {
            return Ok(await _countryRepository.UpdateCountryLocation(model));
        }

        [HttpDelete(nameof(DeleteCountryLocation))]
        [ProducesResponseType(typeof(GenResponse<bool>), 200)]
        public async Task<IActionResult> DeleteCountryLocation(int Id)
        {
            return Ok(await _countryRepository.DeleteCountryLocation(Id));
        }

        #endregion


        #region State Section

        [HttpGet(nameof(FetchAllStates))]
        [ProducesResponseType(typeof(GenResponse<List<StateDetail>>), 200)]
        public async Task<IActionResult> FetchAllStates()
        {
            return Ok(await _stateLocationRepo.FetchAllStates());
        }

        [HttpGet(nameof(AdminFetchAllStates) + "/{includeDeletedAndInActive}")]
        [ProducesResponseType(typeof(GenResponse<List<StateDetailsRespDTO>>), 200)]
        public async Task<IActionResult> AdminFetchAllStates(bool includeDeletedAndInActive = true)
        {
            return Ok(await _stateLocationRepo.FetchAllStates(includeDeletedAndInActive));
        }

        [HttpGet(nameof(FetchStateById))]
        [ProducesResponseType(typeof(GenResponse<StateDetailsRespDTO>), 200)]
        public async Task<IActionResult> FetchStateById(int Id)
        {
            return Ok(await _stateLocationRepo.FetchStateById(Id));
        }

        [HttpPost(nameof(AddNewStateLocation))]
        [ProducesResponseType(typeof(GenResponse<int>), 200)]
        public async Task<IActionResult> AddNewStateLocation(StateCreationRequestDTO model)
        {
            return Ok(await _stateLocationRepo.AddNewStateLocation(model));
        }

        [HttpPut(nameof(UpdateStateLocation))]
        [ProducesResponseType(typeof(GenResponse<bool>), 200)]
        public async Task<IActionResult> UpdateStateLocation(UpdateStateDetailDTO model)
        {
            return Ok(await _stateLocationRepo.UpdateStateLocation(model));
        }

        [HttpDelete(nameof(DeleteStateLocation))]
        [ProducesResponseType(typeof(GenResponse<bool>), 200)]
        public async Task<IActionResult> DeleteStateLocation(int Id)
        {
            return Ok(await _stateLocationRepo.DeleteStateLocation(Id));
        }


        #endregion
    }
}
