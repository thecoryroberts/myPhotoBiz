/**
 * Sidebar State Persistence Utility
 * Remembers which sidebar menu sections are expanded using localStorage
 */

(function() {
    'use strict';

    const STORAGE_KEY = 'myPhotoBiz_sidebar_state';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        initSidebarState();
    });

    function initSidebarState() {
        // Get all collapsible sidebar sections
        const sidebarCollapses = document.querySelectorAll('.sidenav-menu .collapse');

        if (sidebarCollapses.length === 0) return;

        // Load saved state from localStorage
        const savedState = loadState();

        // Apply saved state to each section
        sidebarCollapses.forEach(collapse => {
            const collapseId = collapse.id;
            if (!collapseId) return;

            // Restore saved state
            if (savedState[collapseId] === true) {
                // Show the section
                collapse.classList.add('show');

                // Update the toggle link aria-expanded
                const toggleLink = document.querySelector(`[href="#${collapseId}"]`);
                if (toggleLink) {
                    toggleLink.setAttribute('aria-expanded', 'true');
                    toggleLink.classList.remove('collapsed');
                }
            }

            // Listen for state changes
            collapse.addEventListener('shown.bs.collapse', function() {
                saveCollapseState(collapseId, true);
            });

            collapse.addEventListener('hidden.bs.collapse', function() {
                saveCollapseState(collapseId, false);
            });
        });

        // Auto-expand section containing current page
        expandCurrentSection();
    }

    function expandCurrentSection() {
        // Find the active menu item
        const activeLink = document.querySelector('.sidenav-menu .side-nav-link-ref.active');
        if (!activeLink) return;

        // Find the parent collapse section
        const parentCollapse = activeLink.closest('.collapse');
        if (!parentCollapse) return;

        // Expand it if not already expanded
        if (!parentCollapse.classList.contains('show')) {
            const bsCollapse = new bootstrap.Collapse(parentCollapse, {
                toggle: true
            });

            // Save the state
            saveCollapseState(parentCollapse.id, true);
        }
    }

    function saveCollapseState(collapseId, isExpanded) {
        const currentState = loadState();
        currentState[collapseId] = isExpanded;

        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(currentState));
        } catch (e) {
            console.warn('Failed to save sidebar state:', e);
        }
    }

    function loadState() {
        try {
            const saved = localStorage.getItem(STORAGE_KEY);
            return saved ? JSON.parse(saved) : {};
        } catch (e) {
            console.warn('Failed to load sidebar state:', e);
            return {};
        }
    }

    // Expose utility for manual control if needed
    window.SidebarState = {
        clear: function() {
            try {
                localStorage.removeItem(STORAGE_KEY);
            } catch (e) {
                console.warn('Failed to clear sidebar state:', e);
            }
        },
        get: loadState,
        expandAll: function() {
            const sidebarCollapses = document.querySelectorAll('.sidenav-menu .collapse');
            sidebarCollapses.forEach(collapse => {
                if (!collapse.classList.contains('show')) {
                    new bootstrap.Collapse(collapse, { toggle: true });
                }
            });
        },
        collapseAll: function() {
            const sidebarCollapses = document.querySelectorAll('.sidenav-menu .collapse.show');
            sidebarCollapses.forEach(collapse => {
                new bootstrap.Collapse(collapse, { toggle: false });
            });
        }
    };
})();
