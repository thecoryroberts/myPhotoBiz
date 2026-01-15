namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Standardized API response wrapper for consistent JSON responses across the application
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public record ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Optional message describing the result (success or error message)
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// The data payload (null if operation failed or no data to return)
        /// </summary>
        public T? Data { get; init; }

        /// <summary>
        /// List of validation errors or detailed error messages
        /// </summary>
        public List<string>? Errors { get; init; }

        /// <summary>
        /// Create a successful response with data
        /// </summary>
        public static ApiResponse<T> Ok(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Errors = null
            };
        }

        /// <summary>
        /// Create a successful response without data
        /// </summary>
        public static ApiResponse<T> Ok(string message)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = default,
                Errors = null
            };
        }

        /// <summary>
        /// Create a failure response with a single error message
        /// </summary>
        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = null
            };
        }

        /// <summary>
        /// Create a failure response with multiple validation errors
        /// </summary>
        public static ApiResponse<T> Fail(string message, List<string> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors
            };
        }
    }
}
