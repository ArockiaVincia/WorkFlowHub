// =======================================================================
// Add Task Page JavaScript
// =======================================================================

let cachedPageProjects = [];
let allPageEmployees = [];

$(document).ready(async function() {
    if (!isManager() && !isTeamLead()) {
        showToast('Access Denied: Team Lead or Manager privileges required.', 'warning');
        window.location.href = '/Task/ViewTask';
        return;
    }

    await Promise.all([
        loadProjectsDropdown(),
        loadEmployeesDropdown(),
        populateLookupDropdown($('#pageTaskPriority'), 'TaskPriority', '(Select Priority)'),
        populateLookupDropdown($('#pageTaskStatus'), 'TaskStatus', '(Select Status)', 'To Do')
    ]);

    $('#pageTaskTitle, #pageTaskDesc, #pageTaskProject, #pageTaskEmp, #pageTaskPriority, #pageTaskDueDate, #pageTaskStatus').on('input change', function() {
        $(this).removeClass('is-invalid');
    });
});

async function loadProjectsDropdown() {
    try {
        const response = await authFetch('/api/project');
        if (response && response.ok) {
            cachedPageProjects = await response.json();
            const $select = $('#pageTaskProject');
            $select.html('<option value="">(Select Project)</option>');
            cachedPageProjects.forEach(p => {
                $select.append($('<option>', { value: p.id, text: p.name }));
            });
        }
    } catch (err) {
        console.error('Error loading projects for task dropdown:', err);
    }
}

async function loadEmployeesDropdown() {
    try {
        const response = await authFetch('/api/employee');
        if (response && response.ok) {
            allPageEmployees = await response.json();
        }
    } catch (err) {
        console.error('Error loading employees for task dropdown:', err);
    }
}

function onPageTaskProjectChange() {
    const projId = parseInt($('#pageTaskProject').val(), 10);
    const $empSelect = $('#pageTaskEmp');
    $empSelect.html('<option value="">(Select Employee)</option>');

    if (!projId) return;

    const selectedProj = cachedPageProjects.find(p => p.id === projId);
    if (!selectedProj) return;

    const memberNames = (selectedProj.teamMembers || '')
        .split(',')
        .map(m => m.trim().toLowerCase())
        .filter(Boolean);

    const projectEmployees = allPageEmployees.filter(e => {
        const isManager = selectedProj.projectManagerId && e.id === selectedProj.projectManagerId;
        const isMember = memberNames.includes(e.fullName.trim().toLowerCase());
        return isManager || isMember;
    });

    const employeesToShow = projectEmployees.length > 0 ? projectEmployees : allPageEmployees;

    employeesToShow.forEach(e => {
        $empSelect.append($('<option>', {
            value: e.id,
            text: `${e.fullName} (${e.employeeCode})`
        }));
    });
}

async function submitAddTask() {
    const $titleInput = $('#pageTaskTitle');
    const $descInput = $('#pageTaskDesc');
    const $projInput = $('#pageTaskProject');
    const $empInput = $('#pageTaskEmp');
    const $priorityInput = $('#pageTaskPriority');
    const $dateInput = $('#pageTaskDueDate');
    const $statusInput = $('#pageTaskStatus');
    const $btnSubmit = $('#btnSubmitAddTask');

    const title = ($titleInput.val() || '').trim();
    const desc = ($descInput.val() || '').trim();
    const projVal = $projInput.val();
    const projectId = projVal ? parseInt(projVal, 10) : null;
    const empId = parseInt($empInput.val(), 10);
    const priority = $priorityInput.val();
    const dueDate = $dateInput.val();
    const status = $statusInput.val();

    let valid = true;

    if (!title) {
        $titleInput.addClass('is-invalid');
        $('#pageTitleFeedback').text('Task Title is required.');
        valid = false;
    } else if (title.length > 200) {
        $titleInput.addClass('is-invalid');
        $('#pageTitleFeedback').text('Task Title cannot exceed 200 characters.');
        valid = false;
    } else {
        $titleInput.removeClass('is-invalid');
    }

    if (desc && desc.length > 500) {
        $descInput.addClass('is-invalid');
        $('#pageDescFeedback').text('Description cannot exceed 500 characters.');
        valid = false;
    } else {
        $descInput.removeClass('is-invalid');
    }

    if (!projectId) {
        $projInput.addClass('is-invalid');
        $('#pageProjFeedback').text('Please select a project.');
        valid = false;
    } else {
        $projInput.removeClass('is-invalid');
    }

    if (!empId) {
        $empInput.addClass('is-invalid');
        $('#pageEmpFeedback').text('Please select an employee.');
        valid = false;
    } else {
        $empInput.removeClass('is-invalid');
    }

    if (!priority) {
        $priorityInput.addClass('is-invalid');
        $('#pagePriorityFeedback').text('Please select a priority.');
        valid = false;
    } else {
        $priorityInput.removeClass('is-invalid');
    }

    if (!dueDate) {
        $dateInput.addClass('is-invalid');
        $('#pageDueDateFeedback').text('Due date is required.');
        valid = false;
    } else {
        $dateInput.removeClass('is-invalid');
    }

    if (!status) {
        $statusInput.addClass('is-invalid');
        $('#pageStatusFeedback').text('Please select a status.');
        valid = false;
    } else {
        $statusInput.removeClass('is-invalid');
    }

    if (!valid) return;

    $btnSubmit.prop('disabled', true).text('Saving...');

    try {
        const response = await authFetch('/api/task', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                title: title,
                description: desc,
                projectId: projectId,
                employeeId: empId,
                priority: priority,
                dueDate: dueDate,
                status: status
            })
        });

        const data = await response.json().catch(() => ({}));

        if (response.status === 201) {
            showToast('Task created successfully!', 'success');
            setTimeout(() => {
                window.location.href = '/Task/ViewTask';
            }, 500);
        } else {
            showToast(data.message || 'Failed to create task.', 'danger');
            $btnSubmit.prop('disabled', false).text('Save');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
        $btnSubmit.prop('disabled', false).text('Save');
    }
}
