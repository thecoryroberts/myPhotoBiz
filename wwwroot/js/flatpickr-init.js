/**
 * Flatpickr Date Picker Initialization
 * Provides consistent date picker styling across the application
 */
(function() {
    'use strict';

    // Default Flatpickr configuration
    const defaultConfig = {
        dateFormat: 'Y-m-d',
        altInput: true,
        altFormat: 'F j, Y',
        allowInput: true,
        animate: true,
        disableMobile: false
    };

    // Configuration for datetime inputs
    const dateTimeConfig = {
        ...defaultConfig,
        enableTime: true,
        dateFormat: 'Y-m-d H:i',
        altFormat: 'F j, Y at h:i K',
        time_24hr: false
    };

    // Configuration for time-only inputs
    const timeConfig = {
        enableTime: true,
        noCalendar: true,
        dateFormat: 'H:i',
        altInput: true,
        altFormat: 'h:i K',
        time_24hr: false
    };

    /**
     * Initialize Flatpickr on date inputs
     */
    function initializeFlatpickr() {
        if (typeof flatpickr === 'undefined') {
            console.warn('Flatpickr library not loaded');
            return;
        }

        // Initialize date inputs (type="date")
        document.querySelectorAll('input[type="date"]:not(.flatpickr-input)').forEach(input => {
            // Skip if already initialized
            if (input._flatpickr) return;

            const config = { ...defaultConfig };

            // Check for min/max attributes
            if (input.min) config.minDate = input.min;
            if (input.max) config.maxDate = input.max;

            // Initialize and store reference
            const fp = flatpickr(input, config);
            input._flatpickr = fp;
        });

        // Initialize inputs with data-provider="flatpickr"
        document.querySelectorAll('input[data-provider="flatpickr"]:not(.flatpickr-input)').forEach(input => {
            if (input._flatpickr) return;

            let config = { ...defaultConfig };

            // Check for datetime mode
            if (input.dataset.enableTime === 'true' || input.type === 'datetime-local') {
                config = { ...dateTimeConfig };
            }

            // Check for time-only mode
            if (input.dataset.noCalendar === 'true' || input.type === 'time') {
                config = { ...timeConfig };
            }

            // Check for custom format
            if (input.dataset.dateFormat) {
                config.dateFormat = input.dataset.dateFormat;
            }
            if (input.dataset.altFormat) {
                config.altFormat = input.dataset.altFormat;
            }

            // Check for min/max dates
            if (input.dataset.minDate || input.min) {
                config.minDate = input.dataset.minDate || input.min;
            }
            if (input.dataset.maxDate || input.max) {
                config.maxDate = input.dataset.maxDate || input.max;
            }

            // Check for default date
            if (input.dataset.defaultDate) {
                config.defaultDate = input.dataset.defaultDate;
            }

            const fp = flatpickr(input, config);
            input._flatpickr = fp;
        });

        // Initialize datetime-local inputs
        document.querySelectorAll('input[type="datetime-local"]:not(.flatpickr-input)').forEach(input => {
            if (input._flatpickr) return;

            const config = { ...dateTimeConfig };
            if (input.min) config.minDate = input.min;
            if (input.max) config.maxDate = input.max;

            const fp = flatpickr(input, config);
            input._flatpickr = fp;
        });

        // Initialize time inputs
        document.querySelectorAll('input[type="time"]:not(.flatpickr-input)').forEach(input => {
            if (input._flatpickr) return;

            const fp = flatpickr(input, { ...timeConfig });
            input._flatpickr = fp;
        });
    }

    /**
     * Reinitialize Flatpickr (useful after dynamic content loads)
     */
    window.reinitializeFlatpickr = function() {
        initializeFlatpickr();
    };

    /**
     * Create a date picker programmatically
     * @param {HTMLElement|string} element - Element or selector
     * @param {Object} options - Custom options to merge with defaults
     * @returns {Object} Flatpickr instance
     */
    window.createDatePicker = function(element, options = {}) {
        if (typeof element === 'string') {
            element = document.querySelector(element);
        }
        if (!element) {
            console.warn('Element not found for date picker');
            return null;
        }

        const config = { ...defaultConfig, ...options };
        return flatpickr(element, config);
    };

    /**
     * Create a datetime picker programmatically
     */
    window.createDateTimePicker = function(element, options = {}) {
        if (typeof element === 'string') {
            element = document.querySelector(element);
        }
        if (!element) {
            console.warn('Element not found for datetime picker');
            return null;
        }

        const config = { ...dateTimeConfig, ...options };
        return flatpickr(element, config);
    };

    /**
     * Create a time picker programmatically
     */
    window.createTimePicker = function(element, options = {}) {
        if (typeof element === 'string') {
            element = document.querySelector(element);
        }
        if (!element) {
            console.warn('Element not found for time picker');
            return null;
        }

        const config = { ...timeConfig, ...options };
        return flatpickr(element, config);
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeFlatpickr);
    } else {
        initializeFlatpickr();
    }

    // Reinitialize when modals are shown (for Bootstrap modals)
    document.addEventListener('shown.bs.modal', function(e) {
        // Small delay to ensure modal content is rendered
        setTimeout(initializeFlatpickr, 100);
    });

    // Expose for manual reinitialization
    window.initializeFlatpickr = initializeFlatpickr;
})();
