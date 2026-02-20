using Microsoft.AspNetCore.Mvc;
using geoback.Services;
using geoback.DTOs;

namespace geoback.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacilityController : ControllerBase
    {
        private readonly IFacilityService _facilityService;
        private readonly ILogger<FacilityController> _logger;

        public FacilityController(
            IFacilityService facilityService,
            ILogger<FacilityController> logger)
        {
            _facilityService = facilityService;
            _logger = logger;
        }

        [HttpGet("{ibpsNumber}")]
        public async Task<ActionResult<FacilityDto>> GetFacility(string ibpsNumber)
        {
            try
            {
                var facility = await _facilityService.GetFacilityByIBPSNumberAsync(ibpsNumber);
                if (facility == null)
                    return NotFound($"Facility with IBPS number {ibpsNumber} not found");

                return Ok(facility);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving facility {IBPSNumber}", ibpsNumber);
                return StatusCode(500, "An error occurred while retrieving the facility");
            }
        }

        [HttpGet("{ibpsNumber}/validate")]
        public async Task<ActionResult<bool>> ValidateIBPSNumber(string ibpsNumber)
        {
            try
            {
                var isValid = await _facilityService.ValidateIBPSNumberAsync(ibpsNumber);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating IBPS number {IBPSNumber}", ibpsNumber);
                return StatusCode(500, "An error occurred while validating the IBPS number");
            }
        }
    }
}