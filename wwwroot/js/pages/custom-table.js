/**
 * Custom Table Component
 * Provides client-side filtering, searching, sorting, pagination, and selection for tables
 *
 * Data attributes:
 * - [data-table]: Container element for the table functionality
 * - [data-table-rows-per-page]: Default rows per page (on container)
 * - [data-table-search]: Text search input
 * - [data-table-filter="columnName"]: Dropdown filter for a specific column
 * - [data-table-filter-column="columnName"]: Marks which column to filter (on th element)
 * - [data-filter-value="value"]: The value to match for filtering (on td element)
 * - [data-table-set-rows-per-page]: Dropdown to change rows per page
 * - [data-table-rows-per-page-select]: Alternative attribute for rows per page selector
 * - [data-table-pagination]: Container for pagination buttons
 * - [data-table-pagination-info]: Element to show "Showing X-Y of Z" text
 * - [data-table-pagination-prev]: Previous page button
 * - [data-table-pagination-next]: Next page button
 * - [data-table-select-all]: Checkbox to select all visible rows
 * - [data-table-delete-selected]: Button to delete selected rows
 * - [data-table-sort]: Sortable column header
 * - [data-table-delete-row]: Button to delete a single row
 * - [data-table-row]: Row element with filterable data attributes
 * - [data-status], [data-role], [data-search], [data-name]: Row data for filtering
 */

class CustomTable {
    constructor(container) {
        this.container = container;
        this.table = container.querySelector('table');
        this.tbody = this.table?.querySelector('tbody');
        this.allRows = this.tbody ? Array.from(this.tbody.querySelectorAll('tr[data-table-row], tr:not([data-table-row])')) : [];
        this.filteredRows = [...this.allRows];
        this.currentPage = 1;
        this.rowsPerPage = parseInt(container.getAttribute('data-table-rows-per-page')) || 10;
        this.sortColumn = null;
        this.sortDirection = 'asc';
        this.activeFilters = {};
        this.searchTerm = '';

        this.init();
    }

    init() {
        this.setupSearch();
        this.setupFilters();
        this.setupRowsPerPage();
        this.setupPagination();
        this.setupSelectAll();
        this.setupDeleteSelected();
        this.setupSorting();
        this.setupDeleteRow();

        // Initial render
        this.applyFiltersAndSearch();
    }

    // Search functionality
    setupSearch() {
        const searchInput = this.container.querySelector('[data-table-search]');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.searchTerm = e.target.value.toLowerCase().trim();
                this.currentPage = 1;
                this.applyFiltersAndSearch();
            });
        }
    }

    // Filter functionality
    setupFilters() {
        const filterSelects = this.container.querySelectorAll('[data-table-filter]');
        filterSelects.forEach(select => {
            const filterColumn = select.getAttribute('data-table-filter');
            select.addEventListener('change', (e) => {
                const value = e.target.value;
                // Clear filter if empty, "All", or matches first option (default)
                const firstOption = select.querySelector('option')?.value;
                const isAllOrDefault = !value || value === 'All' || value === '' ||
                    (firstOption && value === firstOption && (firstOption.toLowerCase().includes('all') || firstOption === ''));

                if (isAllOrDefault) {
                    delete this.activeFilters[filterColumn];
                } else {
                    this.activeFilters[filterColumn] = value;
                }
                this.currentPage = 1;
                this.applyFiltersAndSearch();
            });
        });
    }

    // Rows per page functionality
    setupRowsPerPage() {
        const rowsSelects = this.container.querySelectorAll('[data-table-set-rows-per-page], [data-table-rows-per-page-select]');
        rowsSelects.forEach(select => {
            // Set initial value
            const options = select.querySelectorAll('option');
            options.forEach(opt => {
                if (parseInt(opt.value) === this.rowsPerPage) {
                    opt.selected = true;
                }
            });

            select.addEventListener('change', (e) => {
                this.rowsPerPage = parseInt(e.target.value);
                this.currentPage = 1;
                this.renderTable();
            });
        });
    }

    // Pagination functionality
    setupPagination() {
        const prevBtn = this.container.querySelector('[data-table-pagination-prev]');
        const nextBtn = this.container.querySelector('[data-table-pagination-next]');

        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                if (this.currentPage > 1) {
                    this.currentPage--;
                    this.renderTable();
                }
            });
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                const totalPages = Math.ceil(this.filteredRows.length / this.rowsPerPage);
                if (this.currentPage < totalPages) {
                    this.currentPage++;
                    this.renderTable();
                }
            });
        }
    }

    // Select all functionality
    setupSelectAll() {
        const selectAllCheckbox = this.container.querySelector('[data-table-select-all]');
        if (selectAllCheckbox) {
            selectAllCheckbox.addEventListener('change', (e) => {
                const isChecked = e.target.checked;
                const visibleRows = this.getVisibleRows();
                visibleRows.forEach(row => {
                    const checkbox = row.querySelector('input[type="checkbox"]');
                    if (checkbox) {
                        checkbox.checked = isChecked;
                    }
                });
                this.updateDeleteSelectedButton();
            });
        }

        // Also setup individual checkbox listeners
        if (this.tbody) {
            this.tbody.addEventListener('change', (e) => {
                if (e.target.type === 'checkbox') {
                    this.updateDeleteSelectedButton();
                    this.updateSelectAllState();
                }
            });
        }
    }

    updateSelectAllState() {
        const selectAllCheckbox = this.container.querySelector('[data-table-select-all]');
        if (!selectAllCheckbox) return;

        const visibleRows = this.getVisibleRows();
        const checkboxes = visibleRows.map(row => row.querySelector('input[type="checkbox"]')).filter(Boolean);
        const checkedCount = checkboxes.filter(cb => cb.checked).length;

        if (checkedCount === 0) {
            selectAllCheckbox.checked = false;
            selectAllCheckbox.indeterminate = false;
        } else if (checkedCount === checkboxes.length) {
            selectAllCheckbox.checked = true;
            selectAllCheckbox.indeterminate = false;
        } else {
            selectAllCheckbox.checked = false;
            selectAllCheckbox.indeterminate = true;
        }
    }

    updateDeleteSelectedButton() {
        const deleteBtn = this.container.querySelector('[data-table-delete-selected]');
        if (!deleteBtn) return;

        const selectedCount = this.getSelectedRows().length;
        if (selectedCount > 0) {
            deleteBtn.classList.remove('d-none');
            deleteBtn.textContent = `Delete (${selectedCount})`;
        } else {
            deleteBtn.classList.add('d-none');
        }
    }

    getSelectedRows() {
        if (!this.tbody) return [];
        const checkboxes = this.tbody.querySelectorAll('input[type="checkbox"]:checked');
        return Array.from(checkboxes).map(cb => cb.closest('tr')).filter(Boolean);
    }

    // Delete selected functionality
    setupDeleteSelected() {
        const deleteBtn = this.container.querySelector('[data-table-delete-selected]');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', () => {
                const selectedRows = this.getSelectedRows();
                if (selectedRows.length === 0) {
                    alert('Please select items to delete.');
                    return;
                }

                if (confirm(`Are you sure you want to delete ${selectedRows.length} selected item(s)?`)) {
                    selectedRows.forEach(row => {
                        const index = this.allRows.indexOf(row);
                        if (index > -1) {
                            this.allRows.splice(index, 1);
                        }
                        row.remove();
                    });
                    this.applyFiltersAndSearch();
                    this.updateDeleteSelectedButton();
                }
            });
        }
    }

    // Sorting functionality
    setupSorting() {
        const sortHeaders = this.container.querySelectorAll('[data-table-sort]');
        sortHeaders.forEach(header => {
            header.style.cursor = 'pointer';
            header.addEventListener('click', () => {
                const column = header.getAttribute('data-table-sort') || header.textContent.trim().toLowerCase();

                // Toggle direction if same column
                if (this.sortColumn === column) {
                    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
                } else {
                    this.sortColumn = column;
                    this.sortDirection = 'asc';
                }

                // Update header visual indicators
                sortHeaders.forEach(h => {
                    h.classList.remove('sorted-asc', 'sorted-desc');
                });
                header.classList.add(`sorted-${this.sortDirection}`);

                this.sortRows();
                this.renderTable();
            });
        });
    }

    sortRows() {
        if (!this.sortColumn) return;

        this.filteredRows.sort((a, b) => {
            let aVal = this.getSortValue(a, this.sortColumn);
            let bVal = this.getSortValue(b, this.sortColumn);

            // Handle numeric values
            const aNum = parseFloat(aVal);
            const bNum = parseFloat(bVal);
            if (!isNaN(aNum) && !isNaN(bNum)) {
                return this.sortDirection === 'asc' ? aNum - bNum : bNum - aNum;
            }

            // Handle dates
            const aDate = new Date(aVal);
            const bDate = new Date(bVal);
            if (!isNaN(aDate.getTime()) && !isNaN(bDate.getTime())) {
                return this.sortDirection === 'asc' ? aDate - bDate : bDate - aDate;
            }

            // Handle strings
            aVal = String(aVal).toLowerCase();
            bVal = String(bVal).toLowerCase();
            if (this.sortDirection === 'asc') {
                return aVal.localeCompare(bVal);
            } else {
                return bVal.localeCompare(aVal);
            }
        });
    }

    getSortValue(row, column) {
        // First try to get from data attribute
        const dataAttr = row.getAttribute(`data-${column}`);
        if (dataAttr) return dataAttr;

        // Try to find by data-sort attribute on anchor/element
        const sortEl = row.querySelector(`[data-sort="${column}"]`);
        if (sortEl) return sortEl.textContent.trim();

        // Fallback to column index based on header
        const headers = this.table.querySelectorAll('thead th');
        let columnIndex = -1;
        headers.forEach((h, i) => {
            const sortAttr = h.getAttribute('data-table-sort');
            const headerText = h.textContent.trim().toLowerCase();
            if (sortAttr === column || headerText === column) {
                columnIndex = i;
            }
        });

        if (columnIndex >= 0) {
            const cell = row.querySelectorAll('td')[columnIndex];
            return cell ? cell.textContent.trim() : '';
        }

        return '';
    }

    // Delete row functionality
    setupDeleteRow() {
        this.container.addEventListener('click', (e) => {
            const deleteBtn = e.target.closest('[data-table-delete-row]');
            if (deleteBtn) {
                e.preventDefault();
                const row = deleteBtn.closest('tr');
                if (row && confirm('Are you sure you want to delete this item?')) {
                    const index = this.allRows.indexOf(row);
                    if (index > -1) {
                        this.allRows.splice(index, 1);
                    }
                    row.remove();
                    this.applyFiltersAndSearch();
                }
            }
        });
    }

    // Apply filters and search
    applyFiltersAndSearch() {
        this.filteredRows = this.allRows.filter(row => {
            // Apply search
            if (this.searchTerm) {
                const searchData = this.getRowSearchData(row);
                if (!searchData.toLowerCase().includes(this.searchTerm)) {
                    return false;
                }
            }

            // Apply filters
            for (const [filterColumn, filterValue] of Object.entries(this.activeFilters)) {
                const rowValue = this.getRowFilterValue(row, filterColumn);
                if (!rowValue || !rowValue.toLowerCase().includes(filterValue.toLowerCase())) {
                    return false;
                }
            }

            return true;
        });

        // Re-sort if sorting is active
        if (this.sortColumn) {
            this.sortRows();
        }

        this.renderTable();
    }

    getRowSearchData(row) {
        // First check for explicit data-search attribute
        const searchAttr = row.getAttribute('data-search');
        if (searchAttr) return searchAttr;

        // Fallback to all text content
        return row.textContent;
    }

    getRowFilterValue(row, filterColumn) {
        // Normalize column name (lowercase, no special characters)
        const normalizedColumn = filterColumn.toLowerCase();
        // Also check singular/plural variations
        const columnVariations = [
            normalizedColumn,
            normalizedColumn.endsWith('s') ? normalizedColumn.slice(0, -1) : normalizedColumn + 's'
        ];

        // Check data attributes (data-status, data-role, etc.)
        for (const col of columnVariations) {
            const dataAttr = row.getAttribute(`data-${col}`);
            if (dataAttr) return dataAttr;
        }

        // Check for data-filter-value in cells
        const filterCell = row.querySelector(`[data-filter-value]`);
        if (filterCell) {
            // Find the corresponding header to check column match
            const cellIndex = Array.from(row.children).indexOf(filterCell);
            const headers = this.table.querySelectorAll('thead th');
            const header = headers[cellIndex];
            if (header) {
                const headerFilterColumn = header.getAttribute('data-table-filter-column');
                if (headerFilterColumn && columnVariations.includes(headerFilterColumn.toLowerCase())) {
                    return filterCell.getAttribute('data-filter-value');
                }
            }
            // If no specific column match required, use the filter value
            return filterCell.getAttribute('data-filter-value');
        }

        // Try to find column by header
        const headers = this.table.querySelectorAll('thead th');
        let columnIndex = -1;
        headers.forEach((h, i) => {
            const headerFilterColumn = h.getAttribute('data-table-filter-column');
            const dataColumn = h.getAttribute('data-column');
            const headerText = h.textContent.trim().toLowerCase();

            // Check filter column, data-column attribute, or header text
            const headerMatches = columnVariations.some(col =>
                (headerFilterColumn && headerFilterColumn.toLowerCase() === col) ||
                (dataColumn && dataColumn.toLowerCase() === col) ||
                headerText === col
            );

            if (headerMatches) {
                columnIndex = i;
            }
        });

        if (columnIndex >= 0) {
            const cell = row.querySelectorAll('td')[columnIndex];
            if (cell) {
                // Check for data-filter-value on the cell
                const cellFilterValue = cell.getAttribute('data-filter-value');
                if (cellFilterValue) return cellFilterValue;
                // Fallback to text content
                return cell.textContent.trim();
            }
        }

        return '';
    }

    getVisibleRows() {
        const startIndex = (this.currentPage - 1) * this.rowsPerPage;
        const endIndex = startIndex + this.rowsPerPage;
        return this.filteredRows.slice(startIndex, endIndex);
    }

    // Render the table with current state
    renderTable() {
        if (!this.tbody) return;

        // Hide all rows first
        this.allRows.forEach(row => {
            row.style.display = 'none';
        });

        // Show only the visible rows for current page
        const visibleRows = this.getVisibleRows();
        visibleRows.forEach(row => {
            row.style.display = '';
        });

        // Update pagination info
        this.updatePaginationInfo();

        // Update pagination buttons
        this.updatePaginationButtons();

        // Reset select all state
        const selectAllCheckbox = this.container.querySelector('[data-table-select-all]');
        if (selectAllCheckbox) {
            selectAllCheckbox.checked = false;
            selectAllCheckbox.indeterminate = false;
        }

        // Update delete selected button
        this.updateDeleteSelectedButton();
    }

    updatePaginationInfo() {
        const paginationInfo = this.container.querySelector('[data-table-pagination-info]');
        if (!paginationInfo) return;

        const totalItems = this.filteredRows.length;
        const startIndex = totalItems === 0 ? 0 : (this.currentPage - 1) * this.rowsPerPage + 1;
        const endIndex = Math.min(this.currentPage * this.rowsPerPage, totalItems);

        const itemName = paginationInfo.getAttribute('data-table-pagination-info') || 'items';
        paginationInfo.textContent = `Showing ${startIndex}-${endIndex} of ${totalItems} ${itemName}`;
    }

    updatePaginationButtons() {
        const totalPages = Math.ceil(this.filteredRows.length / this.rowsPerPage);

        // Update prev/next buttons
        const prevBtn = this.container.querySelector('[data-table-pagination-prev]');
        const nextBtn = this.container.querySelector('[data-table-pagination-next]');

        if (prevBtn) {
            prevBtn.disabled = this.currentPage <= 1;
        }
        if (nextBtn) {
            nextBtn.disabled = this.currentPage >= totalPages;
        }

        // Generate pagination numbers if container exists
        const paginationContainer = this.container.querySelector('[data-table-pagination]');
        if (paginationContainer && !prevBtn && !nextBtn) {
            paginationContainer.innerHTML = '';

            if (totalPages <= 1) return;

            // Create pagination buttons
            const createButton = (page, text, isActive = false, isDisabled = false) => {
                const btn = document.createElement('button');
                btn.className = `btn btn-sm ${isActive ? 'btn-primary' : 'btn-light'} mx-1`;
                btn.textContent = text || page;
                btn.disabled = isDisabled;
                if (!isDisabled && !isActive) {
                    btn.addEventListener('click', () => {
                        this.currentPage = page;
                        this.renderTable();
                    });
                }
                return btn;
            };

            // Previous button
            const prevButton = createButton(this.currentPage - 1, '«', false, this.currentPage <= 1);
            paginationContainer.appendChild(prevButton);

            // Page numbers
            const maxVisiblePages = 5;
            let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
            let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

            if (endPage - startPage + 1 < maxVisiblePages) {
                startPage = Math.max(1, endPage - maxVisiblePages + 1);
            }

            if (startPage > 1) {
                paginationContainer.appendChild(createButton(1, '1'));
                if (startPage > 2) {
                    const ellipsis = document.createElement('span');
                    ellipsis.className = 'mx-1';
                    ellipsis.textContent = '...';
                    paginationContainer.appendChild(ellipsis);
                }
            }

            for (let i = startPage; i <= endPage; i++) {
                paginationContainer.appendChild(createButton(i, null, i === this.currentPage));
            }

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) {
                    const ellipsis = document.createElement('span');
                    ellipsis.className = 'mx-1';
                    ellipsis.textContent = '...';
                    paginationContainer.appendChild(ellipsis);
                }
                paginationContainer.appendChild(createButton(totalPages, totalPages.toString()));
            }

            // Next button
            const nextButton = createButton(this.currentPage + 1, '»', false, this.currentPage >= totalPages);
            paginationContainer.appendChild(nextButton);
        }
    }
}

// Initialize all custom tables when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    const tableContainers = document.querySelectorAll('[data-table]');
    tableContainers.forEach(container => {
        new CustomTable(container);
    });

    // Also handle tables that use data-table on card wrapper but have table-responsive wrapper
    const cardContainers = document.querySelectorAll('.card:has(table)');
    cardContainers.forEach(card => {
        // Check if already initialized via [data-table]
        if (card.hasAttribute('data-table') || card.querySelector('[data-table]')) {
            return;
        }

        // Check if there are custom table attributes present
        const hasCustomTableFeatures = card.querySelector('[data-table-search], [data-table-filter], [data-table-pagination]');
        if (hasCustomTableFeatures) {
            new CustomTable(card);
        }
    });
});

// Export for external use
if (typeof window !== 'undefined') {
    window.CustomTable = CustomTable;
}
