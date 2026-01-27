/**
 * Real-time Field Validation
 * Provides immediate feedback on blur for common field types
 *
 * Features:
 * - Email validation with format checking
 * - Date validation with range checking
 * - Phone number format validation
 * - Required field validation
 * - Custom pattern validation
 * - Character limit display
 *
 * Usage:
 * - Automatically initializes on all forms
 * - Add data-validate="email|phone|date|required" for specific validation
 * - Add data-min-date, data-max-date for date range validation
 * - Add data-show-count="true" for character counter on textareas
 */

(function() {
    'use strict';

    const FieldValidation = {
        config: {
            errorClass: 'is-invalid',
            successClass: 'is-valid',
            feedbackClass: 'invalid-feedback',
            validFeedbackClass: 'valid-feedback',
            charCountClass: 'char-count text-muted small mt-1'
        },

        patterns: {
            email: /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/,
            phone: /^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$/,
            url: /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/
        },

        messages: {
            required: 'This field is required',
            email: 'Please enter a valid email address',
            phone: 'Please enter a valid phone number',
            url: 'Please enter a valid URL',
            minLength: 'Must be at least {min} characters',
            maxLength: 'Must be no more than {max} characters',
            datePast: 'Date must be in the past',
            dateFuture: 'Date must be in the future',
            dateRange: 'Date must be between {min} and {max}',
            dateInvalid: 'Please enter a valid date',
            pattern: 'Please match the requested format'
        },

        /**
         * Initialize validation for all forms
         */
        init: function() {
            document.querySelectorAll('form').forEach(form => {
                this.initForm(form);
            });

            // Handle dynamically added forms
            this.observeNewForms();
        },

        /**
         * Initialize validation for a single form
         */
        initForm: function(form) {
            if (form.dataset.fieldValidationInit) return;
            form.dataset.fieldValidationInit = 'true';

            // Find all validatable fields
            const fields = form.querySelectorAll('input, textarea, select');

            fields.forEach(field => {
                this.initField(field);
            });
        },

        /**
         * Initialize validation for a single field
         */
        initField: function(field) {
            if (field.dataset.fieldInit) return;
            field.dataset.fieldInit = 'true';

            const type = field.type;
            const validateAttr = field.dataset.validate;

            // Determine validation types
            const validations = this.getFieldValidations(field);

            if (validations.length === 0) return;

            // Add blur event listener
            field.addEventListener('blur', () => {
                this.validateField(field, validations);
            });

            // Add input event for real-time feedback on errors
            field.addEventListener('input', () => {
                if (field.classList.contains(this.config.errorClass)) {
                    this.validateField(field, validations);
                }

                // Update character count if enabled
                if (field.dataset.showCount === 'true') {
                    this.updateCharCount(field);
                }
            });

            // Initialize character count display
            if (field.dataset.showCount === 'true') {
                this.initCharCount(field);
            }
        },

        /**
         * Get all validation types for a field
         */
        getFieldValidations: function(field) {
            const validations = [];
            const type = field.type;
            const validateAttr = field.dataset.validate;

            // Check for explicit validation attribute
            if (validateAttr) {
                validateAttr.split('|').forEach(v => {
                    validations.push(v.trim());
                });
            }

            // Auto-detect based on input type
            if (type === 'email' && !validations.includes('email')) {
                validations.push('email');
            }
            if ((type === 'tel') && !validations.includes('phone')) {
                validations.push('phone');
            }
            if ((type === 'url') && !validations.includes('url')) {
                validations.push('url');
            }
            if ((type === 'date' || type === 'datetime-local') && !validations.includes('date')) {
                validations.push('date');
            }

            // Check for required attribute
            if (field.required && !validations.includes('required')) {
                validations.push('required');
            }

            // Check for min/max length
            if (field.minLength > 0) {
                validations.push('minLength');
            }
            if (field.maxLength > 0 && field.maxLength < 524288) { // Ignore default huge maxLength
                validations.push('maxLength');
            }

            // Check for pattern
            if (field.pattern) {
                validations.push('pattern');
            }

            return validations;
        },

        /**
         * Validate a field against its validation types
         */
        validateField: function(field, validations) {
            const value = field.value.trim();
            let isValid = true;
            let errorMessage = '';

            for (const validation of validations) {
                const result = this.runValidation(field, validation, value);
                if (!result.valid) {
                    isValid = false;
                    errorMessage = result.message;
                    break;
                }
            }

            this.showFieldState(field, isValid, errorMessage);
            return isValid;
        },

        /**
         * Run a single validation check
         */
        runValidation: function(field, validation, value) {
            switch (validation) {
                case 'required':
                    return {
                        valid: value.length > 0,
                        message: this.messages.required
                    };

                case 'email':
                    if (value.length === 0) return { valid: true }; // Empty is OK if not required
                    return {
                        valid: this.patterns.email.test(value),
                        message: this.messages.email
                    };

                case 'phone':
                    if (value.length === 0) return { valid: true };
                    return {
                        valid: this.patterns.phone.test(value),
                        message: this.messages.phone
                    };

                case 'url':
                    if (value.length === 0) return { valid: true };
                    return {
                        valid: this.patterns.url.test(value),
                        message: this.messages.url
                    };

                case 'date':
                    return this.validateDate(field, value);

                case 'minLength':
                {
                    const minLen = parseInt(field.minLength);
                    if (value.length === 0) return { valid: true };
                    return {
                        valid: value.length >= minLen,
                        message: this.messages.minLength.replace('{min}', minLen)
                    };
                }

                case 'maxLength':
                {
                    const maxLen = parseInt(field.maxLength);
                    return {
                        valid: value.length <= maxLen,
                        message: this.messages.maxLength.replace('{max}', maxLen)
                    };
                }

                case 'pattern':
                {
                    if (value.length === 0) return { valid: true };
                    const pattern = new RegExp(field.pattern);
                    return {
                        valid: pattern.test(value),
                        message: field.title || this.messages.pattern
                    };
                }

                default:
                    return { valid: true };
            }
                    return {
                        valid: pattern.test(value),
                        message: field.title || this.messages.pattern
                    };
                }

                default:
                    return { valid: true };
            }
        },

        /**
         * Validate date fields with range checking
         */
        validateDate: function(field, value) {
            if (value.length === 0) return { valid: true };

            const date = new Date(value);
            if (isNaN(date.getTime())) {
                return { valid: false, message: this.messages.dateInvalid };
            }

            const today = new Date();
            today.setHours(0, 0, 0, 0);

            // Check for past-only dates
            if (field.dataset.datePast === 'true') {
                if (date > today) {
                    return { valid: false, message: this.messages.datePast };
                }
            }

            // Check for future-only dates
            if (field.dataset.dateFuture === 'true') {
                if (date < today) {
                    return { valid: false, message: this.messages.dateFuture };
                }
            }

            // Check for date range
            if (field.dataset.minDate) {
                const minDate = new Date(field.dataset.minDate);
                if (date < minDate) {
                    return {
                        valid: false,
                        message: this.messages.dateRange
                            .replace('{min}', this.formatDate(minDate))
                            .replace('{max}', field.dataset.maxDate ? this.formatDate(new Date(field.dataset.maxDate)) : 'unlimited')
                    };
                }
            }

            if (field.dataset.maxDate) {
                const maxDate = new Date(field.dataset.maxDate);
                if (date > maxDate) {
                    return {
                        valid: false,
                        message: this.messages.dateRange
                            .replace('{min}', field.dataset.minDate ? this.formatDate(new Date(field.dataset.minDate)) : 'unlimited')
                            .replace('{max}', this.formatDate(maxDate))
                    };
                }
            }

            return { valid: true };
        },

        /**
         * Format date for display
         */
        formatDate: function(date) {
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });
        },

        /**
         * Show validation state on field
         */
        showFieldState: function(field, isValid, errorMessage) {
            // Remove existing states
            field.classList.remove(this.config.errorClass, this.config.successClass);

            // Remove existing feedback
            const existingFeedback = field.parentElement.querySelector('.' + this.config.feedbackClass);
            if (existingFeedback) {
                existingFeedback.remove();
            }

            if (field.value.trim().length === 0 && !field.required) {
                // Empty non-required field - don't show any state
                return;
            }

            if (isValid) {
                field.classList.add(this.config.successClass);
            } else {
                field.classList.add(this.config.errorClass);

                // Add error message
                const feedback = document.createElement('div');
                feedback.className = this.config.feedbackClass;
                feedback.textContent = errorMessage;

                // Insert after input (or input-group if present)
                const container = field.closest('.input-group') || field;
                container.parentElement.insertBefore(feedback, container.nextSibling);
            }
        },

        /**
         * Initialize character count display
         */
        initCharCount: function(field) {
            const maxLength = field.maxLength;
            if (!maxLength || maxLength >= 524288) return;

            const countEl = document.createElement('div');
            countEl.className = this.config.charCountClass;
            countEl.dataset.charCountFor = field.id || field.name;

            field.parentElement.appendChild(countEl);
            this.updateCharCount(field);
        },

            if (remaining < 0) {
                countEl.classList.add('text-danger');
                countEl.classList.remove('text-warning', 'text-muted');
            } else if (remaining < 20) {
                countEl.classList.add('text-warning');
                countEl.classList.remove('text-danger', 'text-muted');
            } else {
                countEl.classList.add('text-muted');
                countEl.classList.remove('text-warning', 'text-danger');
            }
            const current = field.value.length;
            const remaining = maxLength - current;

            countEl.textContent = `${current}/${maxLength} characters`;

            if (remaining < 20) {
                countEl.classList.add('text-warning');
                countEl.classList.remove('text-danger', 'text-muted');
            } else if (remaining < 0) {
                countEl.classList.add('text-danger');
                countEl.classList.remove('text-warning', 'text-muted');
            } else {
                countEl.classList.add('text-muted');
                countEl.classList.remove('text-warning', 'text-danger');
            }
        },

        /**
         * Observe DOM for new forms
         */
        observeNewForms: function() {
            const observer = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === 1) {
                            if (node.tagName === 'FORM') {
                                this.initForm(node);
                            }
                            node.querySelectorAll?.('form')?.forEach(form => {
                                this.initForm(form);
                            });
                        }
                    });
                });
            });

            observer.observe(document.body, { childList: true, subtree: true });
        },

        /**
         * Manually validate a form (useful before AJAX submission)
         */
        validateForm: function(form) {
            let isValid = true;
            const fields = form.querySelectorAll('input, textarea, select');

            fields.forEach(field => {
                const validations = this.getFieldValidations(field);
                if (validations.length > 0) {
                    const fieldValid = this.validateField(field, validations);
                    if (!fieldValid) {
                        isValid = false;
                    }
                }
            });

            return isValid;
        },

        /**
         * Clear validation state from a form
         */
        clearValidation: function(form) {
            const fields = form.querySelectorAll('input, textarea, select');
            fields.forEach(field => {
                field.classList.remove(this.config.errorClass, this.config.successClass);
                const feedback = field.parentElement.querySelector('.' + this.config.feedbackClass);
                if (feedback) feedback.remove();
            });
        }
    };

    // Auto-initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function() {
        FieldValidation.init();
    });

    // Expose globally
    window.FieldValidation = FieldValidation;
})();
