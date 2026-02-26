// GeoBack/Hubs/NotificationHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace geoback.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation($"User {userId} connected with role {userRole}");
            
            if (userRole == "RM")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "RMs");
            }
            else if (userRole == "QS")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "QSs");
            }
            else if (userRole == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation($"User {userId} disconnected");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task TestConnection()
    {
        await Clients.Caller.SendAsync("TestResponse", "Connection successful");
    }

    public async Task SendToUser(string userId, object notification)
    {
        await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", notification);
    }

    public async Task SendToGroup(string group, object notification)
    {
        await Clients.Group(group).SendAsync("ReceiveNotification", notification);
    }

    public async Task JoinGroup(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        _logger.LogInformation($"User joined group: {group}");
    }

    public async Task LeaveGroup(string group)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        _logger.LogInformation($"User left group: {group}");
    }

    public async Task ReportStatusChanged(string reportId, string oldStatus, string newStatus)
    {
        await Clients.All.SendAsync("ReportStatusChanged", new
        {
            reportId,
            oldStatus,
            newStatus,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task NewComment(string reportId, string commentId, string userId)
    {
        await Clients.Group($"report-{reportId}").SendAsync("NewComment", new
        {
            reportId,
            commentId,
            userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task DecisionMade(string reportId, string decision, string qsId)
    {
        await Clients.Group($"report-{reportId}").SendAsync("DecisionMade", new
        {
            reportId,
            decision,
            qsId,
            timestamp = DateTime.UtcNow
        });
    }
}