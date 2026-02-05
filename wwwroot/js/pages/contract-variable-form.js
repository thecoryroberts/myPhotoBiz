document.addEventListener("DOMContentLoaded", function () {
    var nameInput = document.getElementById("Name");
    var preview = document.getElementById("placeholder-preview");

    if (!nameInput || !preview) {
        return;
    }

    var updatePreview = function () {
        var value = nameInput.value.trim() || "VariableName";
        preview.textContent = "{{" + value + "}}";
    };

    nameInput.addEventListener("input", updatePreview);
    updatePreview();
});
