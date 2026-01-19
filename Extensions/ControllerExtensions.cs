using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;

namespace MyPhotoBiz.Extensions
{
    /// <summary>
    /// Extension methods for ASP.NET Core controllers to simplify API response creation
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        public static string? GetCurrentUserId(this ControllerBase controller)
        {
            return controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Checks if user is in any staff role (Admin or Photographer)
        /// </summary>
        public static bool IsStaffUser(this ControllerBase controller)
        {
            return controller.User.IsInRole(AppConstants.Roles.Admin) ||
                   controller.User.IsInRole(AppConstants.Roles.Photographer);
        }

        /// <summary>
        /// Checks if user is an admin
        /// </summary>
        public static bool IsAdmin(this ControllerBase controller)
        {
            return controller.User.IsInRole(AppConstants.Roles.Admin);
        }

        /// <summary>
        /// Return a standardized success response with data
        /// </summary>
        public static IActionResult ApiOk<T>(this ControllerBase controller, T data, string? message = null)
        {
            return controller.Ok(ApiResponse<T>.Ok(data, message));
        }

        /// <summary>
        /// Return a standardized success response without data
        /// </summary>
        public static IActionResult ApiOk(this ControllerBase controller, string message)
        {
            return controller.Ok(ApiResponse<object>.Ok(message));
        }

        /// <summary>
        /// Return a standardized bad request response with a single error message
        /// </summary>
        public static IActionResult ApiBadRequest<T>(this ControllerBase controller, string message)
        {
            return controller.BadRequest(ApiResponse<T>.Fail(message));
        }

        /// <summary>
        /// Return a standardized bad request response with validation errors
        /// </summary>
        public static IActionResult ApiBadRequest<T>(this ControllerBase controller, string message, List<string> errors)
        {
            return controller.BadRequest(ApiResponse<T>.Fail(message, errors));
        }

        /// <summary>
        /// Return a standardized not found response
        /// </summary>
        public static IActionResult ApiNotFound<T>(this ControllerBase controller, string message = "Resource not found")
        {
            return controller.NotFound(ApiResponse<T>.Fail(message));
        }

        /// <summary>
        /// Return a standardized unauthorized response
        /// </summary>
        public static IActionResult ApiUnauthorized<T>(this ControllerBase controller, string message = "Unauthorized access")
        {
            return controller.Unauthorized(ApiResponse<T>.Fail(message));
        }

        /// <summary>
        /// Return a standardized internal server error response
        /// </summary>
        public static IActionResult ApiError<T>(this ControllerBase controller, string message = "An error occurred")
        {
            return controller.StatusCode(500, ApiResponse<T>.Fail(message));
        }
    }
}
