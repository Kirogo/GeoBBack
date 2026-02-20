namespace geoback.Services
{
    public interface INotificationService
    {
        Task SendInAppNotificationAsync(string userId, string title, string message, string link = null);
        Task SendEmailNotificationAsync(string email, string subject, string body);
        Task NotifyQSsNewReportAsync(int reportId, string ibpsNumber);
        Task NotifyRMReportStatusChangedAsync(int reportId, string rmUserId, string newStatus, string comments);
        Task NotifyQSReportReturnedAsync(int reportId, string qsUserId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendInAppNotificationAsync(string userId, string title, string message, string link = null)
        {
            // TODO: Implement with SignalR
            _logger.LogInformation($"In-app notification to {userId}: {title} - {message}");
            await Task.CompletedTask;
        }

        public async Task SendEmailNotificationAsync(string email, string subject, string body)
        {
            // TODO: Implement with SMTP
            _logger.LogInformation($"Email to {email}: {subject} - {body}");
            await Task.CompletedTask;
        }

        public async Task NotifyQSsNewReportAsync(int reportId, string ibpsNumber)
        {
            _logger.LogInformation($"New report {reportId} for IBPS {ibpsNumber} needs QS review");
            await Task.CompletedTask;
        }

        public async Task NotifyRMReportStatusChangedAsync(int reportId, string rmUserId, string newStatus, string comments)
        {
            _logger.LogInformation($"Report {reportId} status changed to {newStatus} for RM {rmUserId}");
            await Task.CompletedTask;
        }

        public async Task NotifyQSReportReturnedAsync(int reportId, string qsUserId)
        {
            _logger.LogInformation($"Report {reportId} returned to QS {qsUserId}");
            await Task.CompletedTask;
        }
    }
}