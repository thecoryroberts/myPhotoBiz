(function () {
  var currentForm = null;
  var currentMessage = '';

  document.addEventListener('submit', function (e) {
    var form = e.target;
    if (!(form instanceof HTMLFormElement)) return;
    if (!form.hasAttribute('data-confirm')) return;

    e.preventDefault();
    currentForm = form;
    currentMessage = form.getAttribute('data-confirm-message') || 'Are you sure?';

    var msgEl = document.getElementById('confirmModalMessage');
    if (msgEl) msgEl.textContent = currentMessage;

    var modalEl = document.getElementById('confirmModal');
    if (!modalEl) {
      // Fallback to native confirm if modal not available
      if (window.confirm(currentMessage)) {
        form.removeAttribute('data-confirm');
        form.submit();
      }
      return;
    }

    var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
  });

  document.addEventListener('DOMContentLoaded', function () {
    var confirmBtn = document.getElementById('confirmModalConfirmBtn');
    if (!confirmBtn) return;
    confirmBtn.addEventListener('click', function () {
      if (!currentForm) return;
      // prevent re-triggering handler
      currentForm.removeAttribute('data-confirm');
      // ensure any client-side validation runs
      currentForm.submit();
      var modalEl = document.getElementById('confirmModal');
      if (modalEl) {
        var modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
      }
      currentForm = null;
    });
  });
})();
