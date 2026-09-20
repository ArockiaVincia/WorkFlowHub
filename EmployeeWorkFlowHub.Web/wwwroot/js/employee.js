// =======================================================================
// Employee Management JavaScript - Pure Fetch API (Zero Model Binding)
// Role-Based UI Visibility & JWT Authentication
// =======================================================================

const EMP_API_URL = '/api/employee';
const DEPT_API_URL = '/api/department';
let pendingDeleteEmpId = 0;
let cachedDepartments = [];
let cachedEmployees = [];

$(document).ready(function() {
    applyRoleVisibility();
    loadEmployees();
    loadDepartmentOptions();
    populateLookupDropdown($('#employeeRole'), 'UserRole', '(Select Role)');
    bindEmployeeValidationEvents();
});

function applyRoleVisibility() {
    const $btnAdd = $('#btnAddEmployee');
    if ($btnAdd.length) {
        if (isAdmin()) {
            $btnAdd.removeClass('d-none').addClass('d-flex');
        } else {
            $btnAdd.addClass('d-none').removeClass('d-flex');
        }
    }
}

function toggleEmpPasswordVisibility() {
    const $pwdInput = $('#employeePassword');
    const $pwdIcon = $('#iconEmpPassword');
    if ($pwdInput.length === 0 || $pwdIcon.length === 0) return;
    if ($pwdInput.attr('type') === 'password') {
        $pwdInput.attr('type', 'text');
        $pwdIcon.attr('class', 'bi bi-eye-slash');
    } else {
        $pwdInput.attr('type', 'password');
        $pwdIcon.attr('class', 'bi bi-eye');
    }
}

// Auto-generate employee code with defined format: EMP-1001 (sequential 4-digit number)
function generateEmployeeCode(existingList) {
    const list = existingList || cachedEmployees || [];
    let num = 1001;
    while (list.some(e => e && e.employeeCode && e.employeeCode.trim().toUpperCase() === `EMP-${num}`)) {
        num++;
    }
    return `EMP-${num}`;
}

function generateEmployeeCodeFromName(fullName, existingList) {
    return generateEmployeeCode(existingList);
}

function bindEmployeeValidationEvents() {
    $('#employeeCode, #fullName, #email, #departmentId, #designation, #employeeRole, #employeePassword, #isActive').on('input change', function() {
        $(this).removeClass('is-invalid');
    });

    // Auto-generate employee code on Full Name input for new employees
    $('#fullName').on('input', function() {
        const isAddMode = ($('#employeeId').val() || '0') === '0';
        if (isAddMode) {
            const nameVal = $(this).val();
            if (nameVal && nameVal.trim()) {
                const autoCode = generateEmployeeCodeFromName(nameVal, cachedEmployees);
                $('#employeeCode').val(autoCode).removeClass('is-invalid');
            } else {
                $('#employeeCode').val('');
            }
        }
    });
}

// 1. GET ALL EMPLOYEES
async function loadEmployees() {
    const $tableBody = $('#employeesTableBody');
    $tableBody.html(`
        <tr>
            <td colspan="8" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                Loading employees...
            </td>
        </tr>`);

    try {
        const response = await authFetch(EMP_API_URL);
        if (!response.ok) throw new Error(`HTTP Error ${response.status}`);

        const employees = await response.json();
        cachedEmployees = employees || [];
        renderEmployeeTable(employees);
    } catch (error) {
        console.error('Error loading employees:', error);
        $tableBody.html(`
            <tr>
                <td colspan="8" class="text-center py-4 text-danger">
                    Failed to load employees. Please ensure Web API is running.
                </td>
            </tr>`);
        showToast('Error loading employees: ' + error.message, 'danger');
    }
}

// Load department options for select dropdown
async function loadDepartmentOptions() {
    try {
        const response = await authFetch(DEPT_API_URL);
        if (response && response.ok) {
            cachedDepartments = await response.json();
            populateDepartmentSelect(cachedDepartments);
        }
    } catch (err) {
        console.error('Error loading departments dropdown:', err);
    }
}

function populateDepartmentSelect(departments) {
    const $select = $('#departmentId');
    if ($select.length === 0) return;

    $select.html('<option value="">(Select Department)</option>');
    departments.forEach(d => {
        $select.append($('<option>', { value: d.id, text: d.name }));
    });
}

// 2. RENDER TABLE
function renderEmployeeTable(employees) {
    const $tableBody = $('#employeesTableBody');

    if (!employees || employees.length === 0) {
        $tableBody.html(`
            <tr>
                <td colspan="8" class="text-center py-4 text-muted">
                    No employees found.
                </td>
            </tr>`);
        return;
    }

    const userIsAdmin = isAdmin();
    let rowsHtml = '';
    employees.forEach(emp => {
        const safeCode = escapeHtml(emp.employeeCode);
        const safeName = escapeHtml(emp.fullName);
        const safeEmail = escapeHtml(emp.email);
        const safeDept = escapeHtml(emp.departmentName || 'N/A');
        const safeDesig = escapeHtml(emp.designation);
        
        let roleBadge = '<span class="badge bg-secondary-subtle text-secondary border px-2 py-1">Team Member</span>';
        if (emp.role === 'Manager') {
            roleBadge = '<span class="badge bg-primary-subtle text-primary border border-primary-subtle px-2 py-1">Manager</span>';
        } else if (emp.role === 'Team Lead / Project Lead') {
            roleBadge = '<span class="badge bg-info-subtle text-info border border-info-subtle px-2 py-1">Team Lead</span>';
        }

        const statusBadge = emp.isActive 
            ? '<span class="badge bg-success-subtle text-success border border-success-subtle px-2 py-1">Active</span>'
            : '<span class="badge bg-secondary-subtle text-secondary border px-2 py-1">Inactive</span>';

        let actionsHtml = '';
        if (userIsAdmin) {
            actionsHtml = `
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary px-2 py-1" 
                            onclick="openEditEmployeeModal(${emp.id})" 
                            title="Edit Employee">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger px-2 py-1" 
                            onclick="promptDeleteEmployee(${emp.id}, '${escapeAttr(emp.fullName)}')" 
                            title="Delete Employee">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>`;
        } else {
            actionsHtml = `<span class="badge bg-light text-secondary border">View Only</span>`;
        }

        rowsHtml += `
            <tr>
                <td class="ps-4" style="width: 110px;">
                    ${actionsHtml}
                </td>
                <td class="fw-semibold text-dark">${safeCode}</td>
                <td class="fw-medium text-dark">${safeName}</td>
                <td class="text-muted">${safeEmail}</td>
                <td class="text-dark">${safeDept}</td>
                <td class="text-secondary">${safeDesig}</td>
                <td>${roleBadge}</td>
                <td>${statusBadge}</td>
            </tr>`;
    });

    $tableBody.html(rowsHtml);
}

// 3. ADD MODAL
function openAddEmployeeModal() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        return;
    }

    // Hide Employee Code in Add mode
    $('#divEmployeeCode').hide();
    $('#employeeId').val('0');
    $('#employeeCode').val('').prop('disabled', true).prop('readonly', true);
    $('#fullName').val('');
    $('#email').val('');
    $('#departmentId').val('');
    $('#designation').val('');
    $('#employeeRole').val('Developer / Team Member');
    
    // Show password container only in Add mode
    $('#divEmployeePassword').show();

    $('#employeePassword').val('').attr('type', 'password');
    $('#iconEmpPassword').attr('class', 'bi bi-eye');
    $('#isActive').val('true');

    clearEmployeeValidationErrors();

    $('#empModalTitle').text('Add Employee');
    $('#btnSaveEmployee').text('Save');

    const modal = new bootstrap.Modal($('#employeeModal')[0]);
    modal.show();
}

// 4. EDIT MODAL
async function openEditEmployeeModal(id) {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        return;
    }

    try {
        const response = await authFetch(`${EMP_API_URL}/${id}`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const emp = await response.json();

        $('#employeeId').val(emp.id);
        // Show Employee Code in Edit mode alone in disabled state
        $('#divEmployeeCode').show();
        $('#employeeCode').val(emp.employeeCode).prop('disabled', true).prop('readonly', true);
        $('#fullName').val(emp.fullName);
        $('#email').val(emp.email);
        $('#departmentId').val(emp.departmentId);
        $('#designation').val(emp.designation);
        $('#employeeRole').val(emp.role || 'Developer / Team Member');

        // Password not needed in Edit mode - only in Add mode
        $('#divEmployeePassword').hide();
        $('#employeePassword').val('').attr('type', 'password');

        $('#isActive').val(emp.isActive.toString());

        clearEmployeeValidationErrors();

        $('#empModalTitle').text('Edit Employee');
        $('#btnSaveEmployee').text('Save');

        const modal = new bootstrap.Modal($('#employeeModal')[0]);
        modal.show();
    } catch (err) {
        showToast('Error loading employee details: ' + err.message, 'danger');
    }
}

// 5. SAVE EMPLOYEE (POST / PUT)
async function saveEmployee() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'danger');
        return;
    }

    const id = parseInt($('#employeeId').val(), 10) || 0;
    const isEdit = id > 0;
    const $codeInput = $('#employeeCode');
    const $nameInput = $('#fullName');
    const $emailInput = $('#email');
    const $deptInput = $('#departmentId');
    const $desigInput = $('#designation');
    const $roleInput = $('#employeeRole');
    const $pwdInput = $('#employeePassword');
    const $activeInput = $('#isActive');
    const $btnSave = $('#btnSaveEmployee');

    let code = ($codeInput.val() || '').trim();
    const name = ($nameInput.val() || '').trim();

    // Auto-generate code when adding new employee
    if (!isEdit && name) {
        code = generateEmployeeCodeFromName(name, cachedEmployees);
        $codeInput.val(code);
    }

    const email = ($emailInput.val() || '').trim();
    const deptId = parseInt($deptInput.val(), 10);
    const designation = ($desigInput.val() || '').trim();
    const role = ($roleInput.val() || '').trim() || 'Developer / Team Member';
    const password = ($pwdInput.val() || '').trim();
    const isActive = $activeInput.val() === 'true';

    // Strict Client-Side Validation Control & Max Length checks
    let valid = true;

    if (!code) {
        if (!isEdit && name) {
            code = generateEmployeeCodeFromName(name, cachedEmployees);
            $codeInput.val(code);
        } else {
            $codeInput.addClass('is-invalid');
            $('#codeFeedback').text('Employee Code is required.');
            valid = false;
        }
    } else if (code.length > 15) {
        $codeInput.addClass('is-invalid');
        $('#codeFeedback').text('Employee Code cannot exceed 15 characters.');
        valid = false;
    }

    if (!name) {
        $nameInput.addClass('is-invalid');
        $('#nameFeedback').text('Username / Name is required.');
        valid = false;
    } else if (name.length > 50) {
        $nameInput.addClass('is-invalid');
        $('#nameFeedback').text('Username / Name cannot exceed 50 characters.');
        valid = false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email) {
        $emailInput.addClass('is-invalid');
        $('#emailFeedback').text('Email is required.');
        valid = false;
    } else if (!emailRegex.test(email)) {
        $emailInput.addClass('is-invalid');
        $('#emailFeedback').text('Please enter a valid email address.');
        valid = false;
    }

    if (!deptId || isNaN(deptId)) {
        $deptInput.addClass('is-invalid');
        $('#deptFeedback').text('Please select a department.');
        valid = false;
    }

    if (designation && designation.length > 50) {
        $desigInput.addClass('is-invalid');
        $('#desigFeedback').text('Designation cannot exceed 50 characters.');
        valid = false;
    } else {
        $desigInput.removeClass('is-invalid');
    }

    if (!role) {
        $roleInput.addClass('is-invalid');
        $('#roleFeedback').text('Please select a role.');
        valid = false;
    }

    // Password required only in Add mode
    if (!isEdit) {
        if (!password) {
            $pwdInput.addClass('is-invalid');
            $('#passwordFeedback').text('Password is required.');
            valid = false;
        } else if (password.length < 6) {
            $pwdInput.addClass('is-invalid');
            $('#passwordFeedback').text('Password must be at least 6 characters.');
            valid = false;
        }
    }

    if ($activeInput.val() === '') {
        $activeInput.addClass('is-invalid');
        $('#activeFeedback').text('Please select an active status.');
        valid = false;
    }

    if (!valid) return;

    $btnSave.prop('disabled', true).text('Saving...');

    const url = isEdit ? `${EMP_API_URL}/${id}` : EMP_API_URL;
    const method = isEdit ? 'PUT' : 'POST';

    try {
        const payload = {
            id: id,
            employeeCode: code,
            fullName: name,
            username: name,
            email: email,
            departmentId: deptId,
            designation: designation,
            role: role,
            isActive: isActive
        };

        if (!isEdit) {
            payload.password = password;
        }

        const response = await authFetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await response.json().catch(() => ({}));

        if (response.status === 201 || response.status === 200) {
            const modal = bootstrap.Modal.getInstance($('#employeeModal')[0]);
            if (modal) modal.hide();

            showToast(isEdit ? 'Employee updated successfully!' : 'Employee created successfully!', 'success');
            loadEmployees();
        } else if (response.status === 409) {
            const msg = data.message || 'Duplicate record found.';
            if (msg.toLowerCase().includes('code')) {
                $codeInput.addClass('is-invalid');
                $('#codeFeedback').text(msg);
                $codeInput.focus();
            } else if (msg.toLowerCase().includes('email')) {
                $emailInput.addClass('is-invalid');
                $('#emailFeedback').text(msg);
                $emailInput.focus();
            }
            showToast(msg, 'danger');
        } else {
            showToast(data.message || 'Failed to save employee.', 'danger');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
    } finally {
        $btnSave.prop('disabled', false).text('Save');
    }
}

// 6. DELETE EMPLOYEE
function promptDeleteEmployee(id, name) {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        return;
    }

    pendingDeleteEmpId = id;
    $('#deleteEmpNameText').text(`"${name}"`);

    const modal = new bootstrap.Modal($('#deleteEmpModal')[0]);
    modal.show();
}

async function confirmDeleteEmployee() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'danger');
        return;
    }

    if (!pendingDeleteEmpId) return;

    const $btn = $('#btnConfirmEmpDelete');
    $btn.prop('disabled', true).text('Deleting...');

    try {
        const response = await authFetch(`${EMP_API_URL}/${pendingDeleteEmpId}`, {
            method: 'DELETE'
        });

        const data = await response.json().catch(() => ({}));

        const modal = bootstrap.Modal.getInstance($('#deleteEmpModal')[0]);
        if (modal) modal.hide();

        if (response.status === 200) {
            showToast('Employee deleted successfully!', 'success');
            loadEmployees();
        } else if (response.status === 409) {
            showToast(data.message || 'Cannot delete employee: active tasks are assigned.', 'danger');
        } else {
            showToast(data.message || 'Failed to delete employee.', 'danger');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
    } finally {
        $btn.prop('disabled', false).text('Delete');
        pendingDeleteEmpId = 0;
    }
}

function clearEmployeeValidationErrors() {
    $('#employeeCode, #fullName, #email, #departmentId, #designation, #employeeRole, #employeePassword, #isActive').removeClass('is-invalid');
}

function escapeHtml(text) {
    if (!text) return '';
    return $('<div>').text(text).html();
}

function escapeAttr(text) {
    if (!text) return '';
    return text.replace(/'/g, "\\'").replace(/"/g, '&quot;');
}
