public class ButtonViewModel
{
    public string Type { get; set; } = "button"; // button, submit, reset
    public string Variant { get; set; } = "primary"; // primary, secondary, success, danger, etc.
    public string Size { get; set; } = "md"; // sm, md, lg
    public string Icon { get; set; } // Tabler icon name (without ti-)
    public string Text { get; set; }
    public bool IconOnly { get; set; }
    public bool IsLoading { get; set; }
    public bool IsDisabled { get; set; }
    public string AriaLabel { get; set; }
    public string OnClick { get; set; }
    public string AdditionalClasses { get; set; }
}