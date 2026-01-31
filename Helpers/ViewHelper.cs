namespace MyPhotoBiz.Helpers
{
    public static class ViewHelper
    {
        public static string GetInitials(string? firstName, string? lastName)
        {
            var firstInitial = !string.IsNullOrWhiteSpace(firstName) ? firstName.Trim()[0].ToString() : "";
            var lastInitial = !string.IsNullOrWhiteSpace(lastName) ? lastName.Trim()[0].ToString() : "";
            return $"{firstInitial}{lastInitial}".ToUpperInvariant();
        }
    }
}
