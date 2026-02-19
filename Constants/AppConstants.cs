//TODO Consolidate AppConstants.cs and Helpers/AppConstants.cs into a single file to avoid confusion and maintain a single source of truth for application constants. This will help prevent discrepancies and ensure that all parts of the application are using the same values for roles, pagination, invoice settings, and security parameters.
namespace MyPhotoBiz.Constants
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Client = "Client";
            public const string Photographer = "Photographer";
        }

        public static class Pagination
        {
            public const int DefaultPageSize = 20;
            public const int MaxPageSize = 100;
        }

        public static class InvoiceSettings
        {
            public const int InvoiceNumberMinRange = 1000;
            public const int InvoiceNumberMaxRange = 9999;
        }

        public static class Security
        {
            public const int MinPasswordLength = 12;
            public const int MaxLoginAttempts = 5;
            public const int LockoutMinutes = 15;
        }
    }
}
