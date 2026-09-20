// =======================================================================
// Add Employee Page JavaScript
// =======================================================================

let cachedExistingEmployees = [];

$(document).ready(async function() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        window.location.href = '/Employee/ViewEmployee';
        return;
    }

    await loadDepartmentsDropdown();
    await loadExistingEmployees();
    await populateLookupDropdown($('#pageRole'), 'UserRole', '(Select Role)');
    $('#pageRole').val('Developer / Team Member');

    $('#pageEmpCode, #pageFullName, #pageEmail, #pageDepartmentId, #pageRole, #pageDesignation, #pagePassword, #pageIsActive').on('input change', function() {
        $(this).removeClass('is-invalid');
    });

    // Auto-generate employee code on Full Name input (Pattern: EMP-1001)
    $('#pageFullName').on('input', function() {
        const nameVal = $(this).val();
        if (nameVal && nameVal.trim()) {
            const autoCode = generateEmployeeCodeFromName(nameVal, cachedExistingEmployees);
            $('#pageEmpCode').val(autoCode).removeClass('is-invalid');
        } else {
            $('#pageEmpCode').val('');
        }
    });
});

// Auto-generate employee code with defined format: EMP-1001 (sequential 4-digit number)
function generateEmployeeCode(existingList) {
    const list = existingList || cachedExistingEmployees || [];
    let num = 1001;
    while (list.some(e => e && e.employeeCode && e.employeeCode.trim().toUpperCase() === `EMP-${num}`)) {
        num++;
    }
    return `EMP-${num}`;
}

function generateEmployeeCodeFromName(fullName, existingList) {
    return generateEmployeeCode(existingList);
}

async function loadExistingEmployees() {
    try {
        const response = await authFetch('/api/employee');
        if (response && response.ok) {
            cachedExistingEmployees = await response.json();
        }
    } catch (err) {
        console.error('Error loading existing employees:', err);
    }
}

async function loadDepartmentsDropdown() {
    try {
        const response = await authFetch('/api/department');
        if (response && response.ok) {
            const depts = await response.json();
            const select = $('#pageDepartmentId');
            select.html('<option value="">(Select Department)</option>');
            depts.forEach(d => {
                select.append($('<option>', { value: d.id, text: d.name }));
            });
        }
    } catch (err) {
        console.error('Error loading departments for dropdown:', err);
    }
}

async function submitAddEmployee() {
    const codeInput = $('#pageEmpCode');
    const nameInput = $('#pageFullName');
    const emailInput = $('#pageEmail');
    const deptInput = $('#pageDepartmentId');
    const roleInput = $('#pageRole');
    const desigInput = $('#pageDesignation');
    const pwdInput = $('#pagePassword');
    const activeInput = $('#pageIsActive');
    const btnSubmit = $('#btnSubmitAddEmp');

    const name = nameInput.val() ? nameInput.val().trim() : '';
    let code = codeInput.val() ? codeInput.val().trim() : '';

    if (!code && name) {
        code = generateEmployeeCodeFromName(name, cachedExistingEmployees);
        codeInput.val(code);
    }

    const email = emailInput.val() ? emailInput.val().trim() : '';
    const deptId = parseInt(deptInput.val(), 10);
    const role = roleInput.val() ? roleInput.val().trim() : 'Developer / Team Member';
    const desig = desigInput.val() ? desigInput.val().trim() : '';
    const password = pwdInput.val() ? pwdInput.val().trim() : '';
    const isActive = activeInput.val() === 'true';

    let valid = true;

    if (!name) {
        nameInput.addClass('is-invalid');
        $('#pageNameFeedback').text('Username / Name is required.');
        valid = false;
    } else if (name.length > 50) {
        nameInput.addClass('is-invalid');
        $('#pageNameFeedback').text('Username / Name cannot exceed 50 characters.');
        valid = false;
    } else {
        nameInput.removeClass('is-invalid');
    }

    const isEmailValid = email.length > 5 && email.includes('@') && email.lastIndexOf('.') > email.indexOf('@');
    if (!email) {
        emailInput.addClass('is-invalid');
        $('#pageEmailFeedback').text('Email Address is required.');
        valid = false;
    } else if (!isEmailValid) {
        emailInput.addClass('is-invalid');
        $('#pageEmailFeedback').text('Please enter a valid email address (e.g. name@domain.com).');
        valid = false;
    } else if (email.length > 150) {
        emailInput.addClass('is-invalid');
        $('#pageEmailFeedback').text('Email Address cannot exceed 150 characters.');
        valid = false;
    } else {
        emailInput.removeClass('is-invalid');
    }

    if (!deptId) {
        deptInput.addClass('is-invalid');
        $('#pageDeptFeedback').text('Please select a department.');
        valid = false;
    } else {
        deptInput.removeClass('is-invalid');
    }

    if (!role) {
        roleInput.addClass('is-invalid');
        $('#pageRoleFeedback').text('Please select a role.');
        valid = false;
    } else {
        roleInput.removeClass('is-invalid');
    }

    if (desig && desig.length > 50) {
        desigInput.addClass('is-invalid');
        $('#pageDesigFeedback').text('Designation cannot exceed 50 characters.');
        valid = false;
    } else {
        desigInput.removeClass('is-invalid');
    }

    if (!password) {
        pwdInput.addClass('is-invalid');
        $('#pagePasswordFeedback').text('Password is required.');
        valid = false;
    } else if (password.length < 6) {
        pwdInput.addClass('is-invalid');
        $('#pagePasswordFeedback').text('Password must be at least 6 characters.');
        valid = false;
    } else {
        pwdInput.removeClass('is-invalid');
    }

    if (activeInput.val() === '') {
        activeInput.addClass('is-invalid');
        $('#pageActiveFeedback').text('Please select a status.');
        valid = false;
    } else {
        activeInput.removeClass('is-invalid');
    }

    if (!valid) return;

    btnSubmit.prop('disabled', true).text('Saving...');

    try {
        const response = await authFetch('/api/employee', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                employeeCode: code,
                fullName: name,
                username: name,
                email: email,
                departmentId: deptId,
                role: role,
                designation: desig,
                password: password,
                isActive: isActive
            })
        });

        const data = await response.json().catch(() => ({}));

        if (response.status === 201) {
            showToast('Employee created successfully!', 'success');
            setTimeout(() => {
                window.location.href = '/Employee/ViewEmployee';
            }, 500);
        } else if (response.status === 409) {
            const msg = data.message || 'Duplicate employee code or email address.';
            if (msg.toLowerCase().includes('code')) {
                codeInput.addClass('is-invalid');
                $('#pageCodeFeedback').text(msg);
                codeInput.trigger('focus');
            } else if (msg.toLowerCase().includes('email')) {
                emailInput.addClass('is-invalid');
                $('#pageEmailFeedback').text(msg);
                emailInput.trigger('focus');
            }
            showToast(msg, 'danger');
            btnSubmit.prop('disabled', false).text('Save');
        } else {
            showToast(data.message || 'Failed to create employee.', 'danger');
            btnSubmit.prop('disabled', false).text('Save');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
        btnSubmit.prop('disabled', false).text('Save');
    }
}
