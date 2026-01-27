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

        // Versioned migration: clear persisted state only when version changes.
        // Bump this value when you change sidebar-state behavior.
        // BUMP THIS TO FORCE CLIENTS TO CLEAR OLD SIDEBAR STATE
        const CURRENT_VERSION = 'sidebar_v2';
        try {
            const savedVersion = localStorage.getItem(STORAGE_KEY + '_version');
            if (savedVersion !== CURRENT_VERSION) {
                // Remove any old persisted state so we start fresh for this version
                localStorage.removeItem(STORAGE_KEY);
                localStorage.setItem(STORAGE_KEY + '_version', CURRENT_VERSION);
            }
        } catch (e) {
            console.warn('Failed to apply sidebar state migration:', e);
        }

        // Load saved state from localStorage
        const savedState = loadState();

        // Mark active links by comparing hrefs to current location
        markActiveLinks();

        // Determine which collapse (if any) should be expanded.
        // Prefer the section containing the active link. If none, fall back
        // to a single persisted section. If persisted state contains multiple
        // true entries but none contain the active link, do not auto-expand
        // to avoid many sections opening at once.
        const activeCollapseId = findActiveCollapseId();

        if (activeCollapseId) {
            // Expand only the active section
            const target = document.getElementById(activeCollapseId);
            if (target && !target.classList.contains('show')) {
                new bootstrap.Collapse(target, { toggle: true });
            }
            // Save state so that active section becomes the persisted one
            saveSingleState(activeCollapseId);
        } else {
            // No active section — look at saved state. Only allow one persisted
            // true value; if multiple are found, ignore them to prevent
            // unexpected multiple expansions.
            const trueIds = Object.keys(savedState).filter(id => savedState[id] === true);
            if (trueIds.length === 1) {
                const target = document.getElementById(trueIds[0]);
                if (target && !target.classList.contains('show')) {
                    new bootstrap.Collapse(target, { toggle: true });
                }
            }
        }

        // Listen for state changes on each collapse (after we've applied initial state)
        sidebarCollapses.forEach(collapse => {
            const collapseId = collapse.id;
            if (!collapseId) return;

            collapse.addEventListener('shown.bs.collapse', function() {
                // When a section is shown, persist it as the single expanded section
                saveSingleState(collapseId);
            });

            collapse.addEventListener('hidden.bs.collapse', function() {
                // If the current (persisted) section is hidden, clear persisted state
                const current = loadState();
                if (current[collapseId]) {
                    saveSingleState(null);
                }
            });
        });
    }

    // Mark links matching the current location as active
    function markActiveLinks() {
        const links = document.querySelectorAll('.sidenav-menu a.side-nav-link');
        const currentPath = window.location.pathname.replace(/\/$/, '');

        links.forEach(link => {
            const rawHref = link.getAttribute('href') || '';
            // ignore collapse toggles (hrefs that are only hashes)
            if (rawHref.startsWith('#')) return;
            try {
                const url = new URL(link.href, window.location.origin);
                const linkPath = url.pathname.replace(/\/$/, '');
                if (linkPath === currentPath) {
                    link.classList.add('active');
                } else {
                    link.classList.remove('active');
                }
            } catch (e) {
                // ignore invalid URLs
            }
        });

        // Also mark parent toggles active when a child link is active
        const activeChild = document.querySelector('.sidenav-menu a.side-nav-link.active');
        if (activeChild) {
            const parentCollapse = activeChild.closest('.collapse');
            if (parentCollapse && parentCollapse.id) {
                const toggle = document.querySelector(`[href="#${parentCollapse.id}"]`);
                if (toggle) toggle.classList.add('active');
            }
        }
    }

    function findActiveCollapseId() {
        const activeLink = document.querySelector('.sidenav-menu a.side-nav-link.active');
        if (!activeLink) return null;
        const parentCollapse = activeLink.closest('.collapse');
        return parentCollapse ? parentCollapse.id : null;
    }

    // Persist only a single expanded collapse id to avoid multiple expansions
    function saveSingleState(collapseId) {
        try {
            if (!collapseId) {
                localStorage.removeItem(STORAGE_KEY);
                return;
            }
            const obj = {};
            obj[collapseId] = true;
            localStorage.setItem(STORAGE_KEY, JSON.stringify(obj));
        } catch (e) {
            console.warn('Failed to save sidebar state:', e);
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
