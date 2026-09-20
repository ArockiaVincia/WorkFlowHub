// =======================================================================
// Department Management - Role-Based Visibility & JWT Auth
// Architecture: Fetch API -> Web API -> ADO.NET -> SQL Server
// =======================================================================

const API_BASE_URL = '/api/department';
let pendingDeleteId = 0;

$(document).ready(function() {
    applyRoleVisibility();
    loadDepartments();
});

// Apply Role-Based Visibility for UI elements
function applyRoleVisibility() {
    const $btnAdd = $('#btnAddDepartment');
    if ($btnAdd.length) {
        if (isAdmin()) {
            $btnAdd.removeClass('d-none').addClass('d-flex');
        } else {
            $btnAdd.addClass('d-none').removeClass('d-flex');
        }
    }
}

// 1. GET ALL DEPARTMENTS
async function loadDepartments() {
    const $tableBody = $('#departmentsTableBody');
    $tableBody.html(`
        <tr>
            <td colspan="4" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                Loading departments...
            </td>
        </tr>`);

    try {
        const response = await authFetch(API_BASE_URL);
        if (!response.ok) {
            throw new Error(`HTTP Error ${response.status}`);
        }

        const departments = await response.json();
        renderTable(departments);
    } catch (error) {
        console.error('Error loading departments:', error);
        $tableBody.html(`
            <tr>
                <td colspan="4" class="text-center py-4 text-danger fw-normal">
                    Failed to load departments. Please ensure Web API is running.
                </td>
            </tr>`);
        showToast('Error loading departments: ' + error.message, 'danger');
    }
}

// 2. RENDER TABLE (Actions | Department Name | Employee Count | Project Count)
function renderTable(departments) {
    const $tableBody = $('#departmentsTableBody');

    if (!departments || departments.length === 0) {
        $tableBody.html(`
            <tr>
                <td colspan="4" class="text-center py-4 text-muted">
                    No departments found.
                </td>
            </tr>`);
        return;
    }

    const userIsAdmin = isAdmin();
    let rowsHtml = '';
    departments.forEach(dept => {
        const safeName = escapeHtml(dept.name);
        const attrName = escapeAttr(dept.name);
        const empCount = dept.employeeCount ?? 0;
        const projCount = dept.projectCount ?? 0;

        let actionsHtml = '';
        if (userIsAdmin) {
            actionsHtml = `
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary px-2 py-1" 
                            onclick="openEditModal(${dept.id}, '${attrName}')" 
                            title="Edit Department">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger px-2 py-1" 
                            onclick="promptDelete(${dept.id}, '${attrName}')" 
                            title="Delete Department">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>`;
        } else {
            actionsHtml = `<span class="badge bg-light text-secondary border">View Only</span>`;
        }

        rowsHtml += `
            <tr>
                <td class="ps-4" style="width: 120px;">
                    ${actionsHtml}
                </td>
                <td class="fw-medium text-dark">
                    ${safeName}
                </td>
                <td class="text-dark">
                    ${empCount}
                </td>
                <td class="text-dark">
                    ${projCount}
                </td>
            </tr>`;
    });

    $tableBody.html(rowsHtml);
}

// 3. OPEN MODAL FOR ADD
function openAddModal() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        return;
    }

    $('#departmentId').val('0');
    const $input = $('#departmentName');
    $input.val('').removeClass('is-invalid');

    $('#modalTitle').text('Add Department');
    $('#btnSaveDepartment').text('Save');

    const modal = new bootstrap.Modal($('#departmentModal')[0]);
    modal.show();
    setTimeout(() => $input.focus(), 300);
}

// 4. OPEN MODAL FOR EDIT
function openEditModal(id, name) {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        return;
    }

    $('#departmentId').val(id);
    const $input = $('#departmentName');
    $input.val(name).removeClass('is-invalid');

    $('#modalTitle').text('Edit Department');
    $('#btnSaveDepartment').text('Save');

    const modal = new bootstrap.Modal($('#departmentModal')[0]);
    modal.show();
    setTimeout(() => $input.focus(), 300);
}

// 5. SAVE DEPARTMENT (CREATE OR UPDATE) VIA FETCH API
async function saveDepartment() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'danger');
        return;
    }

    const id = parseInt($('#departmentId').val(), 10) || 0;
    const $input = $('#departmentName');
    const $feedback = $('#nameFeedback');
    const $btnSave = $('#btnSaveDepartment');
    const name = ($input.val() || '').trim();

    // Client-side Validation
    if (!name) {
        $input.addClass('is-invalid');
        $feedback.text('Department Name is required.');
        $input.focus();
        return;
    }

    if (name.length > 50) {
        $input.addClass('is-invalid');
        $feedback.text('Department Name cannot exceed 50 characters.');
        $input.focus();
        return;
    }

    $input.removeClass('is-invalid');
    $btnSave.prop('disabled', true).text('Saving...');

    const isEdit = id > 0;
    const url = isEdit ? `${API_BASE_URL}/${id}` : API_BASE_URL;
    const method = isEdit ? 'PUT' : 'POST';

    try {
        const response = await authFetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id: id, name: name })
        });

        const data = await response.json().catch(() => ({}));

        if (response.status === 201 || response.status === 200) {
            // Close modal
            const modal = bootstrap.Modal.getInstance($('#departmentModal')[0]);
            if (modal) modal.hide();

            showToast(isEdit ? 'Department updated successfully!' : 'Department created successfully!', 'success');
            loadDepartments();
        } else if (response.status === 409) {
            $input.addClass('is-invalid');
            $feedback.text(data.message || 'A department with this name already exists.');
            showToast(data.message || 'Duplicate department name.', 'danger');
        } else {
            showToast(data.message || 'Failed to save department.', 'danger');
        }
    } catch (err) {
        console.error('Error saving department:', err);
        showToast('Error: ' + err.message, 'danger');
    } finally {
        $btnSave.prop('disabled', false).text('Save');
    }
}

// 6. DELETE WITH CONFIRMATION
function promptDelete(id, name) {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        return;
    }

    pendingDeleteId = id;
    $('#deleteDeptNameText').text(`"${name}"`);

    const modal = new bootstrap.Modal($('#deleteModal')[0]);
    modal.show();
}

async function confirmDeleteDepartment() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'danger');
        return;
    }

    if (!pendingDeleteId) return;

    const $btn = $('#btnConfirmDelete');
    $btn.prop('disabled', true).text('Deleting...');

    try {
        const response = await authFetch(`${API_BASE_URL}/${pendingDeleteId}`, {
            method: 'DELETE'
        });

        const data = await response.json().catch(() => ({}));

        const modal = bootstrap.Modal.getInstance($('#deleteModal')[0]);
        if (modal) modal.hide();

        if (response.status === 200) {
            showToast('Department deleted successfully!', 'success');
            loadDepartments();
        } else if (response.status === 409) {
            showToast(data.message || 'Cannot delete department: employees are assigned to it.', 'danger');
        } else {
            showToast(data.message || 'Failed to delete department.', 'danger');
        }
    } catch (err) {
        console.error('Error deleting department:', err);
        showToast('Error: ' + err.message, 'danger');
    } finally {
        $btn.prop('disabled', false).text('Delete');
        pendingDeleteId = 0;
    }
}

// Security: Basic HTML escaping
function escapeHtml(text) {
    if (!text) return '';
    return $('<div>').text(text).html();
}

function escapeAttr(text) {
    if (!text) return '';
    return text.replace(/'/g, "\\'").replace(/"/g, '&quot;');
}
