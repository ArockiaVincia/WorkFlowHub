// =======================================================================
// Add Department Page JavaScript
// =======================================================================

$(document).ready(function() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        window.location.href = '/Department/ViewDepartment';
    }

    // Allow Enter key to trigger submit
    $('#pageDepartmentName').on('keydown', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            submitAddDepartment();
        }
    });

    $('#pageDepartmentName').on('input change', function() {
        $(this).removeClass('is-invalid');
    });
});

async function submitAddDepartment() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'danger');
        return;
    }

    const nameInput = $('#pageDepartmentName');
    const feedback = $('#pageNameFeedback');
    const btnSubmit = $('#btnSubmitAddPage');
    const name = nameInput.val() ? nameInput.val().trim() : '';

    // Client-Side Validation
    if (!name) {
        nameInput.addClass('is-invalid');
        feedback.text('Department Name is required.');
        nameInput.trigger('focus');
        return;
    }

    if (name.length > 50) {
        nameInput.addClass('is-invalid');
        feedback.text('Department Name cannot exceed 50 characters.');
        nameInput.trigger('focus');
        return;
    }

    nameInput.removeClass('is-invalid');
    btnSubmit.prop('disabled', true).text('Saving...');

    try {
        const response = await authFetch('/api/department', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: name })
        });

        const data = await response.json().catch(() => ({}));

        if (response.status === 201) {
            showToast('Department created successfully!', 'success');
            setTimeout(() => {
                window.location.href = '/Department/ViewDepartment';
            }, 500);
        } else if (response.status === 409) {
            nameInput.addClass('is-invalid');
            feedback.text(data.message || 'A department with this name already exists.');
            showToast(data.message || 'Duplicate department name.', 'danger');
            btnSubmit.prop('disabled', false).text('Save');
        } else {
            showToast(data.message || 'Failed to create department.', 'danger');
            btnSubmit.prop('disabled', false).text('Save');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
        btnSubmit.prop('disabled', false).text('Save');
    }
}
