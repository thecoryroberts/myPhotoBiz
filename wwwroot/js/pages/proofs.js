// Proofs Management JavaScript

// Show Proof Details Modal
function showProofDetails(id) {
    $.ajax({
        url: `/Proofs/Details/${id}`,
        type: 'GET',
        success: function (result) {
            $('#proofDetailsModal').html(result);
            const modal = new bootstrap.Modal(document.getElementById('proofDetailsModalElement'));
            modal.show();
        },
        error: function (xhr, status, error) {
            showToast('Error', 'Failed to load proof details.', 'error');
            console.error('Error:', error);
        }
    });
}

// Delete Proof
function deleteProof(id) {
    if (!confirm('Are you sure you want to delete this proof?')) {
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: `/Proofs/Delete/${id}`,
        type: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        data: {
            __RequestVerificationToken: token
        },
        success: function (response) {
            if (response.success) {
                showToast('Success', response.message, 'success');
                setTimeout(() => {
                    location.reload();
                }, 1000);
            } else {
                showToast('Error', response.message, 'error');
            }
        },
        error: function (xhr, status, error) {
            showToast('Error', 'An error occurred while deleting the proof.', 'error');
            console.error('Error:', error);
        }
    });
}

// Bulk Delete Proofs
function bulkDeleteProofs() {
    const selectedProofs = [];
    document.querySelectorAll('.proof-checkbox:checked').forEach(cb => {
        selectedProofs.push(parseInt(cb.value));
    });

    if (selectedProofs.length === 0) {
        showToast('Warning', 'Please select proofs to delete.', 'warning');
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selectedProofs.length} proofs? This action cannot be undone.`)) {
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Proofs/BulkDelete',
        type: 'POST',
        headers: {
            'RequestVerificationToken': token,
            'Content-Type': 'application/json'
        },
        data: JSON.stringify(selectedProofs),
        success: function (response) {
            if (response.success) {
                showToast('Success', response.message, 'success');
                setTimeout(() => {
                    location.reload();
                }, 1500);
            } else {
                showToast('Error', response.message, 'error');
            }
        },
        error: function (xhr, status, error) {
            showToast('Error', 'An error occurred while deleting proofs.', 'error');
            console.error('Error:', error);
        }
    });
}

// Show Gallery Details (Navigate to gallery)
function showGalleryDetails(galleryId) {
    window.location.href = `/Galleries/Details/${galleryId}`;
}

// Toast Notification Utility
function showToast(title, message, type) {
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            "closeButton": true,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "timeOut": "3000"
        };
        toastr[type](message, title);
    } else {
        alert(`${title}: ${message}`);
    }
}
