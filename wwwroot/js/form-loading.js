/**
 * Form Loading States Utility
 * Provides consistent loading state handling for form submissions
 *
 * Usage:
 * 1. Auto-initialization: Add class "form-loading" to any form
 * 2. Manual: FormLoading.init(formElement) or FormLoading.initAll()
 *
 * Button structure (optional - will be auto-wrapped if not present):
 * <button type="submit" class="btn btn-primary">
 *     <span class="btn-text">Save</span>
 *     <span class="btn-loading d-none">
 *         <span class="spinner-border spinner-border-sm me-1"></span>
 *         Saving...
 *     </span>
 * </button>
 */

(function() {
    'use strict';

    const FormLoading = {
        config: {
            loadingClass: 'btn-loading',
            textClass: 'btn-text',
            spinnerHtml: '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>',
            defaultLoadingText: 'Processing...'
        },

        /**
         * Initialize loading state handling for a single form
         */
        init: function(form) {
            if (!form || form.dataset.formLoadingInit) return;

            form.dataset.formLoadingInit = 'true';
            form.addEventListener('submit', this.handleSubmit.bind(this));
        },

        /**
         * Initialize all forms with .form-loading class or specific submit buttons
         */
        initAll: function() {
            // Forms with explicit class
            document.querySelectorAll('form.form-loading').forEach(form => {
                this.init(form);
            });

            // Forms with submit buttons that have btn-text/btn-loading structure
            document.querySelectorAll('form button[type="submit"] .btn-text').forEach(btnText => {
                const form = btnText.closest('form');
                if (form) this.init(form);
            });

            // All forms with a primary submit button (auto-enhance)
            document.querySelectorAll('form:not([data-no-loading])').forEach(form => {
                const submitBtn = form.querySelector('button[type="submit"].btn-primary, button[type="submit"].btn-success');
                if (submitBtn && !submitBtn.querySelector('.btn-loading')) {
                    this.enhanceButton(submitBtn);
                    this.init(form);
                }
            });
        },

        /**
         * Handle form submission
         */
        handleSubmit: function(e) {
            const form = e.target;
            const submitBtn = form.querySelector('button[type="submit"]:not(:disabled)');

            if (!submitBtn) return;

            // Check form validity
            if (!form.checkValidity()) {
                form.classList.add('was-validated');
                return;
            }

            // Prevent double-submission
            if (form.dataset.submitting === 'true') {
                e.preventDefault();
                return;
            }

            form.dataset.submitting = 'true';
            this.showLoading(submitBtn);

            // Reset after timeout (in case of network issues)
            setTimeout(() => {
                this.hideLoading(submitBtn);
                form.dataset.submitting = 'false';
            }, 30000); // 30 second timeout
        },

        /**
         * Enhance a button with loading state structure
         */
        enhanceButton: function(button) {
            if (button.querySelector('.btn-loading')) return;

            const originalText = button.innerHTML;
            const loadingText = button.dataset.loadingText || this.config.defaultLoadingText;

            button.innerHTML = `
                <span class="btn-text">${originalText}</span>
                <span class="btn-loading d-none">
                    ${this.config.spinnerHtml}
                    ${loadingText}
                </span>
            `;
        },

        /**
         * Show loading state on button
         */
        showLoading: function(button) {
            button.disabled = true;
            button.classList.add('disabled');

            const btnText = button.querySelector('.btn-text');
            const btnLoading = button.querySelector('.btn-loading');

            if (btnText && btnLoading) {
                btnText.classList.add('d-none');
                btnLoading.classList.remove('d-none');
            } else {
                // Fallback for buttons without structure
                button.dataset.originalText = button.innerHTML;
                button.innerHTML = `${this.config.spinnerHtml} ${button.dataset.loadingText || this.config.defaultLoadingText}`;
            }
        },

        /**
         * Hide loading state on button
         */
        hideLoading: function(button) {
            button.disabled = false;
            button.classList.remove('disabled');

            const btnText = button.querySelector('.btn-text');
            const btnLoading = button.querySelector('.btn-loading');

            if (btnText && btnLoading) {
                btnText.classList.remove('d-none');
                btnLoading.classList.add('d-none');
            } else if (button.dataset.originalText) {
                button.innerHTML = button.dataset.originalText;
            }
        },

        /**
         * Manually trigger loading state
         */
        start: function(button) {
            this.showLoading(button);
        },

        /**
         * Manually stop loading state
         */
        stop: function(button) {
            this.hideLoading(button);
            const form = button.closest('form');
            if (form) form.dataset.submitting = 'false';
        }
    };

    // Auto-initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function() {
        FormLoading.initAll();
    });

    // Also handle dynamically added forms (for modals, etc.)
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            mutation.addedNodes.forEach(function(node) {
                if (node.nodeType === 1) { // Element node
                    if (node.tagName === 'FORM') {
                        FormLoading.init(node);
                    }
                    node.querySelectorAll?.('form')?.forEach(form => {
                        FormLoading.init(form);
                    });
                }
            });
        });
    });

    observer.observe(document.body, { childList: true, subtree: true });

    // Expose globally
    window.FormLoading = FormLoading;
})();
