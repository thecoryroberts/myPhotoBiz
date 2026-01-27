// Handles simple confirm prompts for forms with `data-confirm` attribute
document.addEventListener('submit', function (e) {
  var form = e.target;
  if (!(form instanceof HTMLFormElement)) return;
  if (!form.hasAttribute('data-confirm')) return;

  e.preventDefault();
  var message = form.getAttribute('data-confirm-message') || 'Are you sure?';
  if (window.confirm(message)) {
    // Submit programmatically to avoid re-triggering this handler
    form.removeAttribute('data-confirm');
    form.submit();
  }
});
