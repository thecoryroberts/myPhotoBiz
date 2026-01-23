using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Extensions;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for notifications.
    /// </summary>
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // API: Get user notifications
        [HttpGet]
        [Route("api/notifications")]
        public async Task<IActionResult> GetNotifications(int limit = 10)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, limit);
            return Json(notifications);
        }

        // API: Get unread count
        [HttpGet]
        [Route("api/notifications/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        // API: Mark notification as read
        [HttpPost]
        [Route("api/notifications/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null || notification.UserId != userId)
                return NotFound();

            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { success = true });
        }

        // API: Mark all as read
        [HttpPost]
        [Route("api/notifications/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true });
        }

        // API: Delete notification
        [HttpDelete]
        [Route("api/notifications/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null || notification.UserId != userId)
                return NotFound();

            await _notificationService.DeleteNotificationAsync(id);
            return Ok(new { success = true });
        }

        // View: Notifications page
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 50);
            return View(notifications);
        }

        // Test endpoint to create sample notifications
        [HttpPost]
        [Route("api/notifications/create-test")]
        public async Task<IActionResult> CreateTestNotifications()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            // Create various test notifications
            await _notificationService.NotifySuccess(
                userId,
                "Welcome to MyPhotoBiz!",
                "Your notification system is now active and working perfectly."
            );

            await _notificationService.NotifyInvoiceCreated(
                userId,
                "INV-001",
                1
            );

            await _notificationService.NotifyPhotoShootCreated(
                userId,
                "Wedding Photography Session",
                1
            );

            await _notificationService.NotifyNewClient(
                userId,
                "John & Sarah Smith",
                1
            );

            await _notificationService.NotifyAlbumCreated(
                userId,
                "Summer Wedding 2025",
                1
            );

            await _notificationService.NotifyWarning(
                userId,
                "Payment Due Soon",
                "Invoice INV-002 is due in 3 days. Please follow up with the client."
            );

            return Ok(new { success = true, message = "Test notifications created successfully" });
        }
    }
}
