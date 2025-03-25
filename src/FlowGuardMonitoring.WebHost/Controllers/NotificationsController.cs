using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models.Notifications;
using FlowGuardMonitoring.WebHost.Models.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class NotificationsController : Controller
{
    private readonly NotificationRepository notificationRepository;
    private readonly IPaginationService<Notification> notificationPagination;
    private readonly ICurrentUser currentUser;

    public NotificationsController(
        NotificationRepository notificationRepository,
        ICurrentUser currentUser,
        IPaginationService<Notification> notificationPagination)
    {
        this.notificationRepository = notificationRepository;
        this.currentUser = currentUser;
        this.notificationPagination = notificationPagination;
    }

    public async Task<IActionResult> Index(string searchValue = "")
    {
        var notifications = await this.notificationRepository.GetPagedAsync(
            1,
            int.MaxValue,
            "NotificationId",
            "asc",
            searchValue,
            this.currentUser.UserId);
        return this.View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> GetNotifications([FromBody] DataTablesRequest request)
    {
        int pageNumber = (request.Start / request.Length) + 1;

        var notifications = await this.notificationPagination.GetPaginatedRecords(
            pageNumber,
            request.Length,
            request.SortColumn,
            request.SortDirection,
            request.SearchValue,
            this.currentUser.UserId);

        var viewModel = notifications.Records.Select(n => new NotificationViewModel
        {
            NotificationId = n.NotificationId,
            Time = n.Date,
            Message = n.Description,
            Type = n.Type,
            IsRead = n.IsRead,
        }).ToList();

        var jsonData = new
        {
            draw = request.Draw,
            recordsFiltered = notifications.TotalRecords,
            recordsTotal = notifications.TotalRecords,
            data = viewModel,
        };

        return this.Ok(jsonData);
    }

    [HttpGet("/api/notifications/recent")]
    public async Task<IActionResult> GetRecentNotifications()
    {
        var notifications = await this.notificationRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 4,
            sortColumn: string.Empty,
            sortDirection: "desc",
            searchValue: string.Empty,
            userId: this.currentUser.UserId);

        return this.Json(notifications);
    }

    [HttpPut("/api/notifications/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        if (notificationId <= 0 || (await this.notificationRepository.GetByIdAsync(notificationId))?.UserId != this.currentUser.UserId)
        {
            return this.BadRequest();
        }

        await this.notificationRepository.MarkAsRead(notificationId);
        return this.Ok();
    }

    [HttpGet("/api/notifications/count")]
    public async Task<IActionResult> CountUnreadNotifications()
    {
        return this.Json(this.notificationRepository.GetUnreadCount(this.currentUser.UserId));
    }
}