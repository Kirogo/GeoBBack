using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using geoback.Data;

namespace geoback.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(ApplicationDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Ok(new
                {
                    connected = canConnect,
                    message = canConnect ? "✅ Successfully connected to MySQL" : "❌ Failed to connect to MySQL",
                    database = _context.Database.GetDbConnection().Database,
                    server = _context.Database.GetDbConnection().DataSource
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return StatusCode(500, new
                {
                    connected = false,
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("facilities")]
        public async Task<IActionResult> GetFacilities()
        {
            try
            {
                var facilities = await _context.Facilities
                    .Include(f => f.Milestones)
                    .Include(f => f.Tranches)
                    .ToListAsync();

                return Ok(new
                {
                    count = facilities.Count,
                    facilities = facilities.Select(f => new
                    {
                        f.Id,
                        f.IBPSNumber,
                        f.CustomerName,
                        f.TotalApprovedAmount,
                        MilestoneCount = f.Milestones?.Count ?? 0,
                        TrancheCount = f.Tranches?.Count ?? 0
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching facilities");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("test-ibps/{ibpsNumber}")]
        public async Task<IActionResult> TestIBPSLookup(string ibpsNumber)
        {
            try
            {
                var facility = await _context.Facilities
                    .Include(f => f.Milestones)
                    .Include(f => f.Tranches)
                    .FirstOrDefaultAsync(f => f.IBPSNumber == ibpsNumber);

                if (facility == null)
                {
                    return Ok(new
                    {
                        found = false,
                        message = $"Facility with IBPS {ibpsNumber} not found in local DB"
                    });
                }

                return Ok(new
                {
                    found = true,
                    facility = new
                    {
                        facility.IBPSNumber,
                        facility.CustomerName,
                        facility.TotalApprovedAmount,
                        facility.ProjectDescription,
                        facility.SiteLatitude,
                        facility.SiteLongitude,
                        Milestones = facility.Milestones?.Select(m => new
                        {
                            m.Description,
                            m.MilestoneOrder,
                            m.AllocatedAmount,
                            m.IsAchieved,
                            m.AchievedDate
                        }),
                        Tranches = facility.Tranches?.Select(t => new
                        {
                            t.TrancheNumber,
                            t.Amount,
                            t.Status
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing IBPS lookup");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}