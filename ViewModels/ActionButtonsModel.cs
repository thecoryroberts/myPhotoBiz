namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// Model for the reusable _ActionButtons partial view
    /// </summary>
    public class ActionButtonsModel
    {
        public int Id { get; set; }
        public string Controller { get; set; } = string.Empty;
        public bool ShowView { get; set; } = true;
        public bool ShowEdit { get; set; } = true;
        public bool ShowDelete { get; set; } = true;
        public string? DeleteDataAttribute { get; set; }
        public string? DeleteDataValue { get; set; }
        public List<ExtraButton>? ExtraButtons { get; set; }

        /// <summary>
        /// Creates action buttons for a standard CRUD entity
        /// </summary>
        public static ActionButtonsModel ForEntity(int id, string controller, string? deleteDataValue = null)
        {
            return new ActionButtonsModel
            {
                Id = id,
                Controller = controller,
                DeleteDataAttribute = deleteDataValue != null ? "data-item-name" : null,
                DeleteDataValue = deleteDataValue
            };
        }

        /// <summary>
        /// Creates view-only action buttons
        /// </summary>
        public static ActionButtonsModel ViewOnly(int id, string controller)
        {
            return new ActionButtonsModel
            {
                Id = id,
                Controller = controller,
                ShowEdit = false,
                ShowDelete = false
            };
        }
    }

    /// <summary>
    /// Extra button configuration for _ActionButtons partial
    /// </summary>
    public class ExtraButton
    {
        public string Action { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Color { get; set; }
    }
}
