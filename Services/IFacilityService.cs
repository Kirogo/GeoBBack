using geoback.Data;
using geoback.DTOs;
using geoback.Models;
using Microsoft.EntityFrameworkCore;

namespace geoback.Services
{
    public interface IFacilityService
    {
        Task<FacilityDto?> GetFacilityByIBPSNumberAsync(string ibpsNumber);
        Task<bool> ValidateIBPSNumberAsync(string ibpsNumber);
        Task<FacilityDto?> SyncFacilityFromCoreBankingAsync(string ibpsNumber);
    }

    public class FacilityService : IFacilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FacilityService> _logger;

        public FacilityService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<FacilityService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<FacilityDto?> GetFacilityByIBPSNumberAsync(string ibpsNumber)
        {
            var facility = await _context.Facilities
                .Include(f => f.Milestones)
                .Include(f => f.Tranches)
                .FirstOrDefaultAsync(f => f.IBPSNumber == ibpsNumber);

            if (facility == null)
            {
                return await SyncFacilityFromCoreBankingAsync(ibpsNumber);
            }

            return MapToDto(facility);
        }

        public async Task<bool> ValidateIBPSNumberAsync(string ibpsNumber)
        {
            return await _context.Facilities.AnyAsync(f => f.IBPSNumber == ibpsNumber);
        }

        public async Task<FacilityDto?> SyncFacilityFromCoreBankingAsync(string ibpsNumber)
        {
            // TODO: Implement actual core banking integration
            _logger.LogInformation("Syncing facility {IBPSNumber} from core banking", ibpsNumber);
            return null;
        }

        private FacilityDto MapToDto(Facility facility)
        {
            return new FacilityDto
            {
                Id = facility.Id,
                IBPSNumber = facility.IBPSNumber,
                CustomerName = facility.CustomerName,
                TotalApprovedAmount = facility.TotalApprovedAmount,
                ProjectDescription = facility.ProjectDescription,
                SiteLatitude = facility.SiteLatitude,
                SiteLongitude = facility.SiteLongitude,
                AllowedGeoFenceRadius = facility.AllowedGeoFenceRadius ?? 100,
                Milestones = facility.Milestones?.Select(m => new MilestoneDto
                {
                    Id = m.Id,
                    Description = m.Description,
                    MilestoneOrder = m.MilestoneOrder,
                    AllocatedAmount = m.AllocatedAmount,
                    IsAchieved = m.IsAchieved,
                    AchievedDate = m.AchievedDate
                }).ToList() ?? new(),
                Tranches = facility.Tranches?.Select(t => new DrawdownTrancheDto
                {
                    Id = t.Id,
                    TrancheNumber = t.TrancheNumber,
                    Amount = t.Amount,
                    RequestDate = t.RequestDate,
                    DisbursementDate = t.DisbursementDate,
                    Status = t.Status
                }).ToList() ?? new()
            };
        }
    }
}