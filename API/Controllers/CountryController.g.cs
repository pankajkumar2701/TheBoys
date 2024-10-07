using Microsoft.AspNetCore.Mvc;
using TheBoys.Models;
using TheBoys.Services;
using TheBoys.Entities;
using TheBoys.Filter;
using TheBoys.Helpers;
using TheBoys.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Task = System.Threading.Tasks.Task;
using TheBoys.Authorization;

namespace TheBoys.Controllers
{
    /// <summary>
    /// Controller responsible for managing country related operations.
    /// </summary>
    /// <remarks>
    /// This Controller provides endpoints for adding, retrieving, updating, and deleting country information.
    /// </remarks>
    [Route("api/country")]
    [Authorize]
    public class CountryController : BaseApiController
    {
        private readonly ICountryService _countryService;

        /// <summary>
        /// Initializes a new instance of the CountryController class with the specified context.
        /// </summary>
        /// <param name="icountryservice">The icountryservice to be used by the controller.</param>
        public CountryController(ICountryService icountryservice)
        {
            _countryService = icountryservice;
        }

        /// <summary>Adds a new country</summary>
        /// <param name="model">The country data to be added</param>
        /// <returns>The result of the operation</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NewRecord))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("Country", Entitlements.Create)]
        public async Task<IActionResult> Post([FromBody] Country model)
        {
            model.TenantId = TenantId;
            model.CreatedBy = UserId;
            model.CreatedOn = DateTime.UtcNow;
            var id = await _countryService.Create(model);
            return Ok(new { id });
        }

        /// <summary>Retrieves a list of countrys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of countrys</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Country>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("Country", Entitlements.Read)]
        public async Task<IActionResult> Get([FromQuery] string filters, string searchTerm, int pageNumber = 1, int pageSize = 10, string sortField = null, string sortOrder = "asc")
        {
            List<FilterCriteria> filterCriteria = null;
            if (pageSize < 1)
            {
                return BadRequest(ExceptionFormatter.ErrorMessage("Page size invalid."));
            }

            if (pageNumber < 1)
            {
                return BadRequest(ExceptionFormatter.ErrorMessage("Page mumber invalid."));
            }

            if (!string.IsNullOrEmpty(filters))
            {
                filterCriteria = JsonHelper.Deserialize<List<FilterCriteria>>(filters);
            }

            var result = await _countryService.Get(filterCriteria, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return Ok(result);
        }

        /// <summary>Retrieves a specific country by its primary key</summary>
        /// <param name="id">The primary key of the country</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The country data</returns>
        [HttpGet]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("Country", Entitlements.Read)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, string fields = null)
        {
            var result = await _countryService.GetById( id, fields);
            return Ok(result);
        }

        /// <summary>Deletes a specific country by its primary key</summary>
        /// <param name="id">The primary key of the country</param>
        /// <returns>The result of the operation</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionStatus))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        [Route("{id:Guid}")]
        [UserAuthorize("Country", Entitlements.Delete)]
        public async Task<IActionResult> DeleteById([FromRoute] Guid id)
        {
            var status = await _countryService.Delete(id);
            return Ok(new { status });
        }

        /// <summary>Updates a specific country by its primary key</summary>
        /// <param name="id">The primary key of the country</param>
        /// <param name="updatedEntity">The country data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPut]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionStatus))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("Country", Entitlements.Update)]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] Country updatedEntity)
        {
            if (id != updatedEntity.Id)
            {
                return BadRequest(ExceptionFormatter.ErrorMessage("Mismatched Id"));
            }

            updatedEntity.TenantId = TenantId;
            updatedEntity.UpdatedBy = UserId;
            updatedEntity.UpdatedOn = DateTime.UtcNow;
            var status = await _countryService.Update(id, updatedEntity);
            return Ok(new { status });
        }

        /// <summary>Updates a specific country by its primary key</summary>
        /// <param name="id">The primary key of the country</param>
        /// <param name="updatedEntity">The country data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPatch]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionStatus))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("Country", Entitlements.Update)]
        public async Task<IActionResult> PatchById(Guid id, [FromBody] JsonPatchDocument<Country> updatedEntity)
        {
            if (updatedEntity == null)
                return BadRequest(ExceptionFormatter.ErrorMessage("Patch document is missing."));
            var status = await _countryService.Patch(id, updatedEntity);
            return Ok(new { status });
        }
    }
}