using MyPhotoBiz.Models;
using MyPhotoBiz.Services;

namespace MyPhotoBiz.Extensions
{
    public static class NotificationExtensions
    {
        // Helper method to create invoice notifications
        public static async Task NotifyInvoiceCreated(
            this INotificationService notificationService,
            string userId,
            string invoiceNumber,
            int invoiceId)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "New Invoice Created",
                Message = $"Invoice {invoiceNumber} has been created successfully.",
                Type = NotificationType.Invoice,
                Link = $"/Invoices/Details/{invoiceId}",
                Icon = "ti-file-invoice"
            });
        }

        public static async Task NotifyInvoicePaid(
            this INotificationService notificationService,
            string userId,
            string invoiceNumber,
            int invoiceId)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "Invoice Paid",
                Message = $"Payment received for invoice {invoiceNumber}.",
                Type = NotificationType.Success,
                Link = $"/Invoices/Details/{invoiceId}",
                Icon = "ti-circle-check"
            });
        }

        // Helper method to create photo shoot notifications
        public static async Task NotifyPhotoShootCreated(
            this INotificationService notificationService,
            string userId,
            string photoShootTitle,
            int photoShootId)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "New Photo Shoot Scheduled",
                Message = $"{photoShootTitle} has been scheduled.",
                Type = NotificationType.PhotoShoot,
                Link = $"/PhotoShoots/Details/{photoShootId}",
                Icon = "ti-camera"
            });
        }

        public static async Task NotifyPhotoShootCompleted(
            this INotificationService notificationService,
            string userId,
            string photoShootTitle,
            int photoShootId)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "Photo Shoot Completed",
                Message = $"{photoShootTitle} has been marked as completed.",
                Type = NotificationType.Success,
                Link = $"/PhotoShoots/Details/{photoShootId}",
                Icon = "ti-circle-check"
            });
        }

        // Helper method to create client notifications
        public static async Task NotifyNewClient(
            this INotificationService notificationService,
            string userId,
            string clientName,
            int clientId)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "New Client Added",
                Message = $"{clientName} has been added to your clients.",
                Type = NotificationType.Client,
                Link = $"/Clients/Details/{clientId}",
                Icon = "ti-users"
            });
        }

        // Helper method to create album notifications
        public static async Task NotifyAlbumCreated(
            this INotificationService notificationService,
            string userId,
            string albumName,
            int albumId)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "New Album Created",
                Message = $"Album '{albumName}' has been created.",
                Type = NotificationType.Album,
                Link = $"/Albums/Details/{albumId}",
                Icon = "ti-photo"
            });
        }

        // General notification helpers
        public static async Task NotifySuccess(
            this INotificationService notificationService,
            string userId,
            string title,
            string message,
            string? link = null)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.Success,
                Link = link,
                Icon = "ti-circle-check"
            });
        }

        public static async Task NotifyWarning(
            this INotificationService notificationService,
            string userId,
            string title,
            string message,
            string? link = null)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.Warning,
                Link = link,
                Icon = "ti-alert-triangle"
            });
        }

        public static async Task NotifyError(
            this INotificationService notificationService,
            string userId,
            string title,
            string message,
            string? link = null)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.Error,
                Link = link,
                Icon = "ti-alert-circle"
            });
        }

        public static async Task NotifyInfo(
            this INotificationService notificationService,
            string userId,
            string title,
            string message,
            string? link = null)
        {
            await notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.Info,
                Link = link,
                Icon = "ti-info-circle"
            });
        }
    }
}
