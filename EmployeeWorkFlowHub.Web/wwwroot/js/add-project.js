// =======================================================================
// Add Project Page JavaScript
// =======================================================================

let cachedAddProjEmployees = [];

$(document).ready(async function() {
    if (!isAdmin()) {
        showToast('Access Denied: Administrator privileges required.', 'warning');
        window.location.href = '/Project/ViewProject';
        return;
    }

    await Promise.all([
        loadDeptsDropdown(),
        loadManagersDropdown(),
        populateLookupDropdown($('#pageProjStatus'), 'ProjectStatus', '(Select Status)', 'Planning')
    ]);

    $('#pageProjName, #pageProjDept, #pageProjManager, #pageProjStatus, #pageProjStartDate, #pageProjEndDate').on('input change', function() {
        $(this).removeClass('is-invalid');
    });
});

async function loadDeptsDropdown() {
    try {
        const response = await authFetch('/api/department');
        if (response && response.ok) {
            const depts = await response.json();
            const select = $('#pageProjDept');
            select.html('<option value="">(Select Department)</option>');
            depts.forEach(d => {
                select.append($('<option>', { value: d.id, text: d.name }));
            });
        }
    } catch (err) {
        console.error('Error loading departments:', err);
    }
}

async function loadManagersDropdown() {
    try {
        const response = await authFetch('/api/employee');
        if (response && response.ok) {
            const rawEmployees = await response.json();
            // Deduplicate employees by ID defensively
            const uniqueEmployees = [];
            const seen = new Set();
            (rawEmployees || []).forEach(e => {
                if (e && e.id && !seen.has(e.id)) {
                    seen.add(e.id);
                    uniqueEmployees.push(e);
                }
            });

            cachedAddProjEmployees = uniqueEmployees;

            // Filter managers: Only employees with 'Manager' role
            const managers = uniqueEmployees.filter(e => (e.role || '').toLowerCase().includes('manager'));
            // Filter team members: Only employees with 'Developer' role
            const developers = uniqueEmployees.filter(e => (e.role || '').toLowerCase().includes('developer'));

            const select = $('#pageProjManager');
            select.html('<option value="">(Select Project Manager)</option>');
            managers.forEach(e => {
                select.append($('<option>', { value: e.id, text: `${e.fullName} (${e.employeeCode})` }));
            });

            // Populate team members dropdown menu with developers only
            renderPageTeamMembersDropdown(developers);
        }
    } catch (err) {
        console.error('Error loading employees for manager dropdown:', err);
    }
}

function renderPageTeamMembersDropdown(employees) {
    const menu = $('#pageProjTeamMembersMenu');
    if (!menu.length) return;

    if (!employees || employees.length === 0) {
        menu.html('<div class="text-muted small px-2 py-1">No employees found.</div>');
        return;
    }

    let html = '';
    employees.forEach(e => {
        const safeName = escapeHtml(e.fullName);
        const safeCode = escapeHtml(e.employeeCode);
        const safeVal = escapeAttr(e.fullName);

        html += `
            <label class="dropdown-item d-flex align-items-center gap-2 py-2 px-3 rounded mb-0" style="cursor: pointer;">
                <input class="form-check-input mt-0 page-team-member-cb" type="checkbox" value="${safeVal}" id="page_tm_cb_${e.id}" onchange="updatePageSelectedTeamMembers()">
                <span class="text-dark">${safeName} <span class="text-muted small">(${safeCode})</span></span>
            </label>`;
    });

    menu.html(html);
}

function updatePageSelectedTeamMembers() {
    const checkboxes = $('.page-team-member-cb:checked');
    const selectedNames = checkboxes.map(function() { return $(this).val().trim(); }).get();

    const textSpan = $('#pageProjTeamMembersText');
    const hiddenInput = $('#pageProjTeamMembers');
    const btn = $('#pageProjTeamMembersBtn');
    const feedback = $('#pageProjMembersFeedback');

    if (selectedNames.length > 0) {
        textSpan.text(selectedNames.join(', '));
        textSpan.removeClass('text-muted').addClass('text-dark fw-medium');
        hiddenInput.val(selectedNames.join(', '));
        btn.removeClass('is-invalid');
        feedback.attr('style', 'display: none !important;').hide();
    } else {
        textSpan.text('(Select Team Members)');
        textSpan.addClass('text-muted').removeClass('text-dark fw-medium');
        hiddenInput.val('');
    }
}

async function submitAddProject() {
    const nameInput = $('#pageProjName');
    const deptInput = $('#pageProjDept');
    const managerInput = $('#pageProjManager');
    const membersHidden = $('#pageProjTeamMembers');
    const membersBtn = $('#pageProjTeamMembersBtn');
    const membersFeedback = $('#pageProjMembersFeedback');
    const statusInput = $('#pageProjStatus');
    const btnSubmit = $('#btnSubmitAddProj');

    const name = nameInput.val() ? nameInput.val().trim() : '';
    const deptId = parseInt(deptInput.val(), 10);
    const managerId = managerInput.val() ? parseInt(managerInput.val(), 10) : null;
    const teamMembers = membersHidden.val() ? membersHidden.val().trim() : null;
    const status = statusInput.val();

    let valid = true;

    // Project Name (required)
    if (!name) {
        nameInput.addClass('is-invalid');
        $('#pageProjNameFeedback').text('Project Name is required.');
        valid = false;
    } else if (name.length > 50) {
        nameInput.addClass('is-invalid');
        $('#pageProjNameFeedback').text('Project Name cannot exceed 50 characters.');
        valid = false;
    } else {
        nameInput.removeClass('is-invalid');
    }

    // Department (required)
    if (!deptId) {
        deptInput.addClass('is-invalid');
        $('#pageProjDeptFeedback').text('Please select a department.');
        valid = false;
    } else {
        deptInput.removeClass('is-invalid');
    }

    // Project Manager (required)
    if (!managerId) {
        managerInput.addClass('is-invalid');
        $('#pageProjManagerFeedback').text('Please select a project manager.');
        valid = false;
    } else {
        managerInput.removeClass('is-invalid');
    }

    // Team Members (required)
    if (!teamMembers) {
        membersBtn.addClass('is-invalid');
        membersFeedback.text('Please select at least one team member.');
        membersFeedback.removeAttr('style').show();
        valid = false;
    } else if (teamMembers.length > 500) {
        membersBtn.addClass('is-invalid');
        membersFeedback.text('Team Members cannot exceed 500 characters.');
        membersFeedback.removeAttr('style').show();
        valid = false;
    } else {
        membersBtn.removeClass('is-invalid');
        membersFeedback.attr('style', 'display: none !important;').hide();
    }

    // Status (required)
    if (!status) {
        statusInput.addClass('is-invalid');
        $('#pageProjStatusFeedback').text('Please select a status.');
        valid = false;
    } else {
        statusInput.removeClass('is-invalid');
    }

    // Start Date & End Date validation
    const startDateInput = $('#pageProjStartDate');
    const endDateInput = $('#pageProjEndDate');
    const startDate = startDateInput.val() || null;
    const endDate = endDateInput.val() || null;

    if (startDate && endDate && new Date(endDate) < new Date(startDate)) {
        endDateInput.addClass('is-invalid');
        $('#pageProjEndDateFeedback').text('End Date cannot be earlier than Start Date.');
        valid = false;
    } else {
        endDateInput.removeClass('is-invalid');
    }

    if (!valid) return;

    btnSubmit.prop('disabled', true).text('Saving...');

    try {
        const response = await authFetch('/api/project', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                name: name,
                departmentId: deptId,
                projectManagerId: managerId,
                teamMembers: teamMembers,
                status: status,
                startDate: startDate,
                endDate: endDate
            })
        });

        const data = await response.json().catch(() => ({}));

        if (response.status === 201) {
            showToast('Project created successfully!', 'success');
            setTimeout(() => {
                window.location.href = '/Project/ViewProject';
            }, 500);
        } else if (response.status === 409) {
            nameInput.addClass('is-invalid');
            $('#pageProjNameFeedback').text(data.message || 'A project with this name already exists.');
            showToast(data.message || 'Duplicate project name.', 'danger');
            btnSubmit.prop('disabled', false).text('Save');
        } else {
            showToast(data.message || 'Failed to create project.', 'danger');
            btnSubmit.prop('disabled', false).text('Save');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
        btnSubmit.prop('disabled', false).text('Save');
    }
}

function escapeHtml(text) {
    if (!text) return '';
    return $('<div>').text(text).html();
}

function escapeAttr(text) {
    if (!text) return '';
    return text.replace(/'/g, "\\'").replace(/"/g, '&quot;');
}
