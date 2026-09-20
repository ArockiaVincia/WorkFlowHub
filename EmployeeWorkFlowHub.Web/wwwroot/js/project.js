// =======================================================================
// Project Management JavaScript - Pure Fetch API (Zero Razor Model Binding)
// Role-Based UI Visibility & JWT Authentication
// =======================================================================

const PROJ_API_URL = '/api/project';
const DEPT_API_URL = '/api/department';
const EMP_API_URL = '/api/employee';
let pendingDeleteProjId = 0;
let cachedProjectEmployees = [];

$(document).ready(function() {
    applyRoleVisibility();
    loadProjects();
    loadProjDepartments();
    loadProjManagers();
    populateLookupDropdown($('#projStatus'), 'ProjectStatus', '(Select Status)');

    $('#projectName, #projDeptId, #projManagerId, #projStatus, #projStartDate, #projEndDate').on('input change', function() {
        $(this).removeClass('is-invalid');
    });
});

function applyRoleVisibility() {
    const $btnAdd = $('#btnAddProject');
    if ($btnAdd.length) {
        if (isManager() || isTeamLead()) {
            $btnAdd.removeClass('d-none').addClass('d-flex');
        } else {
            $btnAdd.addClass('d-none').removeClass('d-flex');
        }
    }
}

/**
 * Loads all projects via GET /api/project and updates the grid.
 */
async function loadProjects() {
    const $tableBody = $('#projectsTableBody');
    $tableBody.html('<tr><td colspan="8" class="text-center py-4 text-muted"><div class="spinner-border spinner-border-sm text-primary me-2"></div>Loading projects...</td></tr>');

    try {
        const response = await authFetch(PROJ_API_URL);
        if (!response.ok) throw new Error(`HTTP Error ${response.status}`);

        const projects = await response.json();
        renderProjectTable(projects);
    } catch (err) {
        console.error('Error loading projects:', err);
        $tableBody.html('<tr><td colspan="8" class="text-center py-4 text-danger">Failed to load projects.</td></tr>');
        showToast('Error loading projects: ' + err.message, 'danger');
    }
}

/**
 * Populates the Department dropdown from GET /api/department.
 */
async function loadProjDepartments() {
    try {
        const response = await authFetch(DEPT_API_URL);
        if (response && response.ok) {
            const depts = await response.json();
            const $select = $('#projDeptId');
            if ($select.length) {
                $select.html('<option value="">(Select Department)</option>');
                depts.forEach(d => {
                    $select.append($('<option>', { value: d.id, text: d.name }));
                });
            }
        }
    } catch (err) {
        console.error('Error loading departments dropdown for projects:', err);
    }
}

/**
 * Populates the Project Manager dropdown from GET /api/employee and Team Members menu.
 */
async function loadProjManagers() {
    try {
        const response = await authFetch(EMP_API_URL);
        if (response && response.ok) {
            const employees = await response.json();
            // Deduplicate employees by ID defensively
            const uniqueEmployees = [];
            const seen = new Set();
            (employees || []).forEach(e => {
                if (e && e.id && !seen.has(e.id)) {
                    seen.add(e.id);
                    uniqueEmployees.push(e);
                }
            });

            cachedProjectEmployees = uniqueEmployees;

            // Filter managers: employees with 'Manager' or 'Lead' role
            const managers = uniqueEmployees.filter(e => (e.role || '').toLowerCase().includes('manager') || (e.role || '').toLowerCase().includes('lead'));
            // Filter team members: Only employees with 'Developer' role
            const developers = uniqueEmployees.filter(e => (e.role || '').toLowerCase().includes('developer'));

            const $select = $('#projManagerId');
            if ($select.length) {
                $select.html('<option value="">(Select Project Manager)</option>');
                managers.forEach(e => {
                    $select.append($('<option>', {
                        value: e.id,
                        text: `${e.fullName} (${e.employeeCode})`
                    }));
                });
            }

            renderTeamMembersDropdown(developers);
        }
    } catch (err) {
        console.error('Error loading project managers dropdown:', err);
    }
}

/**
 * Populates the Team Members dropdown menu with employee checkboxes.
 */
function renderTeamMembersDropdown(employees) {
    const $menu = $('#projTeamMembersMenu');
    if ($menu.length === 0) return;

    if (!employees || employees.length === 0) {
        $menu.html('<div class="px-3 py-2 text-muted small">No employees available</div>');
        return;
    }

    // Deduplicate employees by ID defensively
    const uniqueEmployees = [];
    const seen = new Set();
    employees.forEach(emp => {
        if (emp && emp.id && !seen.has(emp.id)) {
            seen.add(emp.id);
            uniqueEmployees.push(emp);
        }
    });

    let html = '';
    uniqueEmployees.forEach(emp => {
        const safeName = escapeHtml(emp.fullName);
        const safeCode = escapeHtml(emp.employeeCode);
        const valName = escapeAttr(emp.fullName);
        html += `
            <div class="dropdown-item px-3 py-1 d-flex align-items-center gap-2">
                <input class="form-check-input mt-0 team-member-cb" type="checkbox" value="${valName}" id="cb_tm_${emp.id}" onchange="updateSelectedTeamMembers()">
                <label class="form-check-label small w-100 cursor-pointer" for="cb_tm_${emp.id}">
                    ${safeName} <span class="text-muted">(${safeCode})</span>
                </label>
            </div>`;
    });

    $menu.html(html);
}

/**
 * Synchronizes selected checkboxes to the hidden input and button label.
 */
function updateSelectedTeamMembers() {
    const selectedNames = $('.team-member-cb:checked').map(function() {
        return $(this).val();
    }).get();

    const $textSpan = $('#projTeamMembersText');
    const $hiddenInput = $('#projTeamMembers');
    const $btn = $('#projTeamMembersBtn');
    const $feedback = $('#projMembersFeedback');

    if (selectedNames.length > 0) {
        $textSpan.text(selectedNames.join(', ')).removeClass('text-muted').addClass('text-dark fw-medium');
        $hiddenInput.val(selectedNames.join(', '));
        $btn.removeClass('is-invalid');
        $feedback.hide();
    } else {
        $textSpan.text('(Select Team Members)').addClass('text-muted').removeClass('text-dark fw-medium');
        $hiddenInput.val('');
    }
}

/**
 * Renders project table rows with role-based actions.
 */
function renderProjectTable(projects) {
    const $tableBody = $('#projectsTableBody');
    if (!projects || projects.length === 0) {
        $tableBody.html('<tr><td colspan="8" class="text-center py-4 text-muted">No projects found.</td></tr>');
        return;
    }

    const userIsManager = isManager();
    const userIsLead = isTeamLead();
    let rowsHtml = '';
    projects.forEach(p => {
        const safeName = escapeHtml(p.name);
        const safeDept = escapeHtml(p.departmentName || 'N/A');
        const safeManager = p.projectManagerName ? escapeHtml(p.projectManagerName) : '<span class="text-muted fst-italic">Unassigned</span>';
        const safeMembers = p.teamMembers ? escapeHtml(p.teamMembers) : '<span class="text-muted fst-italic">None</span>';

        const formatProjDate = (d) => {
            if (!d) return '<span class="text-muted fst-italic">-</span>';
            const dt = new Date(d);
            return isNaN(dt.getTime()) ? '<span class="text-muted fst-italic">-</span>' : dt.toISOString().split('T')[0];
        };
        const safeStartDate = formatProjDate(p.startDate);
        const safeEndDate = formatProjDate(p.endDate);

        let statusBadge = '<span class="badge bg-secondary-subtle text-secondary border px-2 py-1">Planning</span>';
        if (p.status === 'In Progress') {
            statusBadge = '<span class="badge bg-primary-subtle text-primary border border-primary-subtle px-2 py-1">In Progress</span>';
        } else if (p.status === 'Completed') {
            statusBadge = '<span class="badge bg-success-subtle text-success border border-success-subtle px-2 py-1">Completed</span>';
        }

        let actionsHtml = '';
        if (userIsManager) {
            // Manager: Edit + Delete
            actionsHtml = `
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary px-2 py-1" onclick="openEditProjectModal(${p.id})" title="Edit Project">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger px-2 py-1" onclick="promptDeleteProject(${p.id}, '${escapeAttr(p.name)}')" title="Delete Project">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>`;
        } else if (userIsLead) {
            // Team Lead: Edit and Delete for project assigned to them
            const currentEmpId = getEmployeeId();
            if (p.projectManagerId === currentEmpId) {
                actionsHtml = `
                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-outline-primary px-2 py-1" onclick="openEditProjectModal(${p.id})" title="Edit Project">
                            <i class="bi bi-pencil-square"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger px-2 py-1" onclick="promptDeleteProject(${p.id}, '${escapeAttr(p.name)}')" title="Delete Project">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>`;
            } else {
                actionsHtml = `<span class="badge bg-light text-secondary border">View Only</span>`;
            }
        } else {
            actionsHtml = `<span class="badge bg-light text-secondary border">View Only</span>`;
        }

        rowsHtml += `
            <tr>
                <td class="ps-4" style="width: 110px;">
                    ${actionsHtml}
                </td>
                <td class="fw-medium text-dark">${safeName}</td>
                <td class="text-dark">${safeDept}</td>
                <td class="text-dark">${safeManager}</td>
                <td class="text-secondary small">${safeMembers}</td>
                <td class="text-dark small">${safeStartDate}</td>
                <td class="text-dark small">${safeEndDate}</td>
                <td>${statusBadge}</td>
            </tr>`;
    });

    $tableBody.html(rowsHtml);
}

/**
 * Prepares and displays modal for adding a new project.
 */
function openAddProjectModal() {
    if (!isManager() && !isTeamLead()) {
        showToast('Access Denied: You do not have permission to add projects.', 'warning');
        return;
    }

    $('#projectId').val('0');
    $('#projectName').val('').removeClass('is-invalid');
    
    $('#projDeptId').val('').prop('disabled', false).removeClass('is-invalid');
    if (isTeamLead()) {
        $('#projManagerId').val(getEmployeeId()).prop('disabled', false).removeClass('is-invalid');
    } else {
        $('#projManagerId').val('').prop('disabled', false).removeClass('is-invalid');
    }
    $('#projStatus').val('').removeClass('is-invalid');
    $('#projStartDate').val('').removeClass('is-invalid');
    $('#projEndDate').val('').removeClass('is-invalid');

    // Reset team members checkboxes
    $('.team-member-cb').prop('checked', false);
    updateSelectedTeamMembers();

    $('#projTeamMembersBtn').removeClass('is-invalid');
    $('#projMembersFeedback').hide();

    $('#projModalTitle').text('Add Project');
    $('#btnSaveProject').text('Save');

    const modal = new bootstrap.Modal($('#projectModal')[0]);
    modal.show();
}

/**
 * Loads project details and opens the edit modal.
 */
async function openEditProjectModal(id) {
    if (!isManager() && !isTeamLead()) {
        showToast('Access Denied: You do not have permission to edit projects.', 'warning');
        return;
    }

    try {
        const response = await authFetch(`${PROJ_API_URL}/${id}`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const p = await response.json();

        $('#projectId').val(p.id);
        $('#projectName').val(p.name).removeClass('is-invalid');
        
        $('#projDeptId').val(p.departmentId).prop('disabled', !isManager()).removeClass('is-invalid');
        if (p.projectManagerId && $('#projManagerId option[value="' + p.projectManagerId + '"]').length === 0) {
            const existingMgr = cachedProjectEmployees.find(e => e.id === p.projectManagerId);
            if (existingMgr) {
                $('#projManagerId').append($('<option>', {
                    value: existingMgr.id,
                    text: `${existingMgr.fullName} (${existingMgr.employeeCode})`
                }));
            }
        }
        $('#projManagerId').val(p.projectManagerId || '').prop('disabled', !isManager()).removeClass('is-invalid');
        $('#projStatus').val(p.status || 'In Progress').removeClass('is-invalid');
        $('#projStartDate').val(p.startDate ? p.startDate.split('T')[0] : '').removeClass('is-invalid');
        $('#projEndDate').val(p.endDate ? p.endDate.split('T')[0] : '').removeClass('is-invalid');

        // Pre-check selected team members
        const currentMembers = (p.teamMembers || '')
            .split(',')
            .map(m => m.trim().toLowerCase())
            .filter(Boolean);

        $('.team-member-cb').each(function() {
            $(this).prop('checked', currentMembers.includes($(this).val().trim().toLowerCase()));
        });
        updateSelectedTeamMembers();

        $('#projTeamMembersBtn').removeClass('is-invalid');
        $('#projMembersFeedback').hide();

        $('#projModalTitle').text('Edit Project');
        $('#btnSaveProject').text('Save');

        const modal = new bootstrap.Modal($('#projectModal')[0]);
        modal.show();
    } catch (err) {
        showToast('Error loading project details: ' + err.message, 'danger');
    }
}

/**
 * Saves project (POST or PUT).
 */
async function saveProject() {
    if (!isManager() && !isTeamLead()) {
        showToast('Access Denied: You do not have permission to modify projects.', 'danger');
        return;
    }

    const id = parseInt($('#projectId').val(), 10) || 0;
    if (id === 0 && !isManager()) {
        showToast('Access Denied: Only Managers can create projects.', 'danger');
        return;
    }
    const $nameInput = $('#projectName');
    const $deptInput = $('#projDeptId');
    const $managerInput = $('#projManagerId');
    const $teamMembersInput = $('#projTeamMembers');
    const $statusInput = $('#projStatus');
    const $membersBtn = $('#projTeamMembersBtn');
    const $membersFeedback = $('#projMembersFeedback');
    const $btnSave = $('#btnSaveProject');

    const name = ($nameInput.val() || '').trim();
    const deptId = parseInt($deptInput.val(), 10);
    const managerVal = $managerInput.val();
    const managerId = managerVal ? parseInt(managerVal, 10) : null;
    const teamMembers = ($teamMembersInput.val() || '').trim();
    const status = ($statusInput.val() || '').trim();

    let isValid = true;

    // 1. Project Name (required, max 50)
    if (!name) {
        $nameInput.addClass('is-invalid');
        $('#projNameFeedback').text('Project Name is required.');
        isValid = false;
    } else if (name.length > 50) {
        $nameInput.addClass('is-invalid');
        $('#projNameFeedback').text('Project Name cannot exceed 50 characters.');
        isValid = false;
    } else {
        $nameInput.removeClass('is-invalid');
    }

    // 2. Department (required)
    if (!deptId) {
        $deptInput.addClass('is-invalid');
        $('#projDeptFeedback').text('Please select a department.');
        isValid = false;
    } else {
        $deptInput.removeClass('is-invalid');
    }

    // 3. Project Manager (required)
    if (!managerId) {
        $managerInput.addClass('is-invalid');
        $('#projManagerFeedback').text('Please select a project manager.');
        isValid = false;
    } else {
        $managerInput.removeClass('is-invalid');
    }

    // 4. Team Members (required)
    if (!teamMembers) {
        $membersBtn.addClass('is-invalid');
        $membersFeedback.text('Please select at least one team member.').show();
        isValid = false;
    } else if (teamMembers.length > 500) {
        $membersBtn.addClass('is-invalid');
        $membersFeedback.text('Team Members cannot exceed 500 characters.').show();
        isValid = false;
    } else {
        $membersBtn.removeClass('is-invalid');
        $membersFeedback.hide();
    }

    // 5. Status (required)
    if (!status) {
        $statusInput.addClass('is-invalid');
        $('#projStatusFeedback').text('Please select a status.');
        isValid = false;
    } else {
        $statusInput.removeClass('is-invalid');
    }

    // 6. Start Date & End Date validation
    const $startDateInput = $('#projStartDate');
    const $endDateInput = $('#projEndDate');
    const startDate = $startDateInput.val() || null;
    const endDate = $endDateInput.val() || null;

    if (startDate && endDate && new Date(endDate) < new Date(startDate)) {
        $endDateInput.addClass('is-invalid');
        $('#projEndDateFeedback').text('End Date cannot be earlier than Start Date.');
        isValid = false;
    } else {
        $endDateInput.removeClass('is-invalid');
    }

    if (!isValid) return;

    $btnSave.prop('disabled', true).text('Saving...');

    const isEdit = id > 0;
    const url = isEdit ? `${PROJ_API_URL}/${id}` : PROJ_API_URL;
    const method = isEdit ? 'PUT' : 'POST';

    try {
        const response = await authFetch(url, {
            method: method,
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

        if (response.status === 201 || response.status === 200) {
            const modal = bootstrap.Modal.getInstance($('#projectModal')[0]);
            if (modal) modal.hide();

            showToast(isEdit ? 'Project updated successfully!' : 'Project created successfully!', 'success');
            loadProjects();
        } else if (response.status === 409) {
            $nameInput.addClass('is-invalid');
            $('#projNameFeedback').text(data.message || 'A project with this name already exists.');
            showToast(data.message || 'Duplicate project name.', 'danger');
        } else {
            showToast(data.message || 'Failed to save project.', 'danger');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
    } finally {
        $btnSave.prop('disabled', false).text('Save');
    }
}

/**
 * Prompts confirmation modal to delete project.
 */
function promptDeleteProject(id, name) {
    if (!isManager()) {
        showToast('Access Denied: Only Managers can delete projects.', 'warning');
        return;
    }

    pendingDeleteProjId = id;
    $('#deleteProjNameText').text(`"${name}"`);
    const modal = new bootstrap.Modal($('#deleteProjModal')[0]);
    modal.show();
}

/**
 * Confirms deletion via DELETE /api/project/{id}.
 */
async function confirmDeleteProject() {
    if (!isManager()) {
        showToast('Access Denied: Only Managers can delete projects.', 'danger');
        return;
    }

    if (!pendingDeleteProjId) return;

    try {
        const response = await authFetch(`${PROJ_API_URL}/${pendingDeleteProjId}`, { method: 'DELETE' });
        const modal = bootstrap.Modal.getInstance($('#deleteProjModal')[0]);
        if (modal) modal.hide();

        if (response.status === 200) {
            showToast('Project deleted successfully!', 'success');
            loadProjects();
        } else {
            const data = await response.json().catch(() => ({}));
            showToast(data.message || 'Failed to delete project.', 'danger');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
    } finally {
        pendingDeleteProjId = 0;
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
