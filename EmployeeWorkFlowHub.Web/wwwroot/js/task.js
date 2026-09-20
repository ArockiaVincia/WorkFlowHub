// =======================================================================
// Task Management JavaScript - 8-Stage Kanban Workflow
// Role-Based UI Visibility & JWT Authentication
// =======================================================================

const TASK_API_URL = '/api/task';
const EMP_API_URL = '/api/employee';
const PROJ_API_URL = '/api/project';

// Dynamic Workflow Stages (loaded from Database LookupMaster: TaskStatus)
let WORKFLOW_STAGES = [
    { id: 'colToDo', countId: 'countColToDo', status: 'To Do', title: 'TO DO' },
    { id: 'colTodayTask', countId: 'countColTodayTask', status: 'Today Task', title: 'TODAY TASK' },
    { id: 'colInProgress', countId: 'countColInProgress', status: 'In Progress', title: 'IN PROGRESS' },
    { id: 'colUnderReview', countId: 'countColUnderReview', status: 'Under Review', title: 'UNDER REVIEW' },
    { id: 'colReadyForQC', countId: 'countColReadyForQC', status: 'Ready For QC', title: 'READY FOR QC' },
    { id: 'colUnderQC', countId: 'countColUnderQC', status: 'Under QC / To Be Verify', title: 'UNDER QC /TO BE VERIFY' },
    { id: 'colQCFailed', countId: 'countColQCFailed', status: 'QC Failed', title: 'QC FAILED' },
    { id: 'colDone', countId: 'countColDone', status: 'Done', title: 'DONE' }
];

function getStageId(statusName) {
    return 'col' + (statusName || '').replace(/[^a-zA-Z0-9]/g, '');
}

function getStageCountId(statusName) {
    return 'countCol' + (statusName || '').replace(/[^a-zA-Z0-9]/g, '');
}

async function loadWorkflowStages() {
    try {
        const items = await fetchLookupItems('TaskStatus');
        if (items && items.length > 0) {
            WORKFLOW_STAGES = items.map(item => {
                const val = item.value || item.lookupValue;
                return {
                    id: getStageId(val),
                    countId: getStageCountId(val),
                    status: val,
                    title: val.toUpperCase()
                };
            });
        }
    } catch (err) {
        console.error('Error loading workflow stages from database:', err);
    }
    renderKanbanColumns();
}

function renderKanbanColumns() {
    const $grid = $('#kanbanGrid');
    if (!$grid.length) return;
    $grid.empty();

    WORKFLOW_STAGES.forEach(st => {
        const safeStatus = escapeAttr(st.status);
        const colHtml = `
            <div class="kanban-column" data-status="${safeStatus}">
                <div class="kanban-column-header">
                    <div class="kanban-col-title">${escapeHtml(st.title)}</div>
                    <div class="kanban-col-count" id="${st.countId}">(0)</div>
                </div>
                <div id="${st.id}" class="kanban-column-body" data-status="${safeStatus}">
                    <div class="text-center py-3 text-muted small"><div class="spinner-border spinner-border-sm text-primary me-1"></div>Loading...</div>
                </div>
            </div>`;
        $grid.append(colHtml);
    });
}

function normalizeStatus(status) {
    const s = (status || '').trim().toLowerCase();
    if (s === 'to do' || s === 'todo') return 'To Do';
    if (s === 'today task' || s === 'today') return 'Today Task';
    if (s === 'in progress') return 'In Progress';
    if (s === 'under review') return 'Under Review';
    if (s === 'ready for qc') return 'Ready For QC';
    if (s.includes('qc') && (s.includes('verify') || s.includes('under'))) return 'Under QC / To Be Verify';
    if (s === 'qc failed') return 'QC Failed';
    if (s === 'done' || s === 'completed') return 'Done';
    return 'To Do';
}

function getStageIndex(status) {
    const norm = normalizeStatus(status);
    const idx = WORKFLOW_STAGES.findIndex(st => st.status.toLowerCase() === norm.toLowerCase());
    return idx >= 0 ? idx : 0;
}

// Workflow Status Transition Validator
// Validates status progression, with the explicit exception allowing "QC Failed" -> "Under QC / To Be Verify"
function isValidStatusTransition(currentStatus, targetStatus) {
    const curNorm = normalizeStatus(currentStatus);
    const tarNorm = normalizeStatus(targetStatus);

    if (curNorm === tarNorm) return true;

    // Explicit exception: QC Failed -> Under QC / To Be Verify
    if (curNorm === 'QC Failed' && tarNorm === 'Under QC / To Be Verify') {
        return true;
    }

    const curIdx = getStageIndex(curNorm);
    const tarIdx = getStageIndex(tarNorm);

    return tarIdx >= curIdx;
}

let allTasks = [];
let pendingDeleteTaskId = 0;
let cachedTaskProjects = [];
let allTaskEmployees = [];
let draggedTaskId = null;

$(document).ready(async function() {
    applyRoleVisibility();
    bindKanbanDragDropEvents();
    await loadWorkflowStages();
    await loadTaskDropdowns();
    loadTasks();
    loadTaskProjects();
    loadTaskEmployees();

    $('#taskTitle, #taskDesc, #taskProjectId, #taskEmpId, #taskPriority, #taskDueDate, #taskStatus').on('input change', function() {
        $(this).removeClass('is-invalid');
    });
});

async function loadTaskDropdowns() {
    await Promise.all([
        populateLookupDropdown($('#filterTaskStatus'), 'TaskStatus', '(Select Status)'),
        populateLookupDropdown($('#filterTaskPriority'), 'TaskPriority', '(Select Priority)'),
        populateLookupDropdown($('#taskPriority'), 'TaskPriority', '(Select Priority)'),
        populateLookupDropdown($('#taskStatus'), 'TaskStatus', '(Select Status)')
    ]);
}

function applyRoleVisibility() {
    const $btnAdd = $('#btnAddTask');
    if ($btnAdd.length) {
        if (isManager() || isTeamLead()) {
            $btnAdd.removeClass('d-none').addClass('d-flex');
        } else {
            $btnAdd.addClass('d-none').removeClass('d-flex');
        }
    }
}

// For Team Members: load ONLY projects associated with tasks assigned to them into the filter dropdown
function populateTeamMemberProjects() {
    const $filterSelect = $('#filterTaskProject');
    if ($filterSelect.length === 0) return;

    const currentVal = $filterSelect.val();
    $filterSelect.html('<option value="">(Select Project)</option>');

    const projectMap = new Map();
    allTasks.forEach(t => {
        if (t.projectId && !projectMap.has(t.projectId)) {
            projectMap.set(t.projectId, t.projectName || `Project #${t.projectId}`);
        }
    });

    projectMap.forEach((name, id) => {
        $filterSelect.append($('<option>', { value: id, text: name }));
    });

    if (currentVal && projectMap.has(parseInt(currentVal, 10))) {
        $filterSelect.val(currentVal);
    }
}

async function loadTasks() {
    WORKFLOW_STAGES.forEach(st => {
        $(`#${st.id}`).html('<div class="text-center py-4 text-muted small"><div class="spinner-border spinner-border-sm text-primary me-2"></div>Loading...</div>');
    });

    try {
        const response = await authFetch(TASK_API_URL);
        if (!response.ok) throw new Error(`HTTP Error ${response.status}`);

        allTasks = await response.json();

        // Team Members and QC get only their associated projects loaded in the project filter dropdown
        if (isTeamMember() || isQC()) {
            populateTeamMemberProjects();
        }

        applyTaskFilters();
    } catch (err) {
        console.error('Error loading tasks:', err);
        WORKFLOW_STAGES.forEach(st => {
            $(`#${st.id}`).html('<div class="text-center text-danger py-4 small">Failed to load tasks.</div>');
        });
        showToast('Error loading tasks: ' + err.message, 'danger');
    }
}

async function loadTaskProjects() {
    if (isTeamMember() || isQC()) {
        populateTeamMemberProjects();
        return;
    }

    try {
        const response = await authFetch(PROJ_API_URL);
        if (response && response.ok) {
            const projects = await response.json();
            cachedTaskProjects = projects;

            // Populate Filter Dropdown
            const $filterSelect = $('#filterTaskProject');
            if ($filterSelect.length) {
                $filterSelect.html('<option value="">(Select Project)</option>');
                projects.forEach(p => {
                    $filterSelect.append($('<option>', { value: p.id, text: p.name }));
                });
            }

            // Populate Modal Dropdown
            const $modalSelect = $('#taskProjectId');
            if ($modalSelect.length) {
                $modalSelect.html('<option value="">(Select Project)</option>');
                projects.forEach(p => {
                    $modalSelect.append($('<option>', { value: p.id, text: p.name }));
                });
            }
        }
    } catch (err) {
        console.error('Error loading projects for task dropdowns:', err);
    }
}

async function loadTaskEmployees() {
    if (isTeamMember() || isQC()) return;
    try {
        const response = await authFetch(EMP_API_URL);
        if (response && response.ok) {
            const raw = await response.json();
            const unique = [];
            const seen = new Set();
            (raw || []).forEach(e => {
                if (e && e.id && !seen.has(e.id)) {
                    seen.add(e.id);
                    unique.push(e);
                }
            });
            allTaskEmployees = unique;
        }
    } catch (err) {
        console.error('Error loading employees for tasks:', err);
    }
}

function onTaskProjectChange() {
    const $projSelect = $('#taskProjectId');
    const $empSelect = $('#taskEmpId');
    if ($projSelect.length === 0 || $empSelect.length === 0) return;

    const projId = parseInt($projSelect.val(), 10);
    $empSelect.html('<option value="">(Select Employee)</option>');

    if (!projId) return;

    const selectedProj = cachedTaskProjects.find(p => p.id === projId);
    if (!selectedProj) return;

    const managerId = selectedProj.projectManagerId;
    const teamMemberNames = (selectedProj.teamMembers || '')
        .split(',')
        .map(s => s.trim().toLowerCase())
        .filter(Boolean);

    const filteredEmployees = allTaskEmployees.filter(emp => {
        if (managerId && emp.id === managerId) return true;
        if (teamMemberNames.includes(emp.fullName.trim().toLowerCase())) return true;
        return false;
    });

    if (filteredEmployees.length === 0) {
        $empSelect.append($('<option>', { value: '', text: 'No assigned members found in project', disabled: true }));
        return;
    }

    filteredEmployees.forEach(e => {
        const isManager = managerId && e.id === managerId;
        $empSelect.append($('<option>', {
            value: e.id,
            text: `${e.fullName} (${e.employeeCode})${isManager ? ' - Manager' : ''}`
        }));
    });
}

function applyTaskFilters() {
    const projFilter = $('#filterTaskProject').val();
    const statusFilter = $('#filterTaskStatus').val();
    const priorityFilter = $('#filterTaskPriority').val();

    let filtered = allTasks;

    if (projFilter) {
        filtered = filtered.filter(t => t.projectId == projFilter);
    }
    if (statusFilter) {
        filtered = filtered.filter(t => normalizeStatus(t.status) === normalizeStatus(statusFilter));
    }
    if (priorityFilter) {
        filtered = filtered.filter(t => t.priority === priorityFilter);
    }

    renderKanbanBoard(filtered);
}

function resetTaskFilters() {
    $('#filterTaskProject').val('');
    $('#filterTaskStatus').val('');
    $('#filterTaskPriority').val('');

    renderKanbanBoard(allTasks);
}

function renderKanbanBoard(tasks) {
    const list = tasks || [];

    WORKFLOW_STAGES.forEach(st => {
        const $colEl = $(`#${st.id}`);
        const $countEl = $(`#${st.countId}`);

        const stageTasks = list.filter(t => normalizeStatus(t.status) === st.status);

        if ($countEl.length) {
            $countEl.text(`(${stageTasks.length})`);
        }
        if ($colEl.length) {
            $colEl.html(renderColumnCards(stageTasks));
        }
    });
}

function renderColumnCards(taskList) {
    if (!taskList || taskList.length === 0) {
        return '<div class="text-center text-muted py-4 small fst-italic">No tasks found.</div>';
    }

    const userIsManager = isManager();
    const userIsLead = isTeamLead();
    const userIsMember = isTeamMember();
    const userIsQC = isQC();
    const currentEmpId = getEmployeeId();

    return taskList.map(t => {
        const safeTitle = escapeHtml(t.title);
        const safeProject = t.projectName 
            ? escapeHtml(t.projectName) 
            : '<span class="text-muted fst-italic">No Project</span>';
        const safeEmp = escapeHtml(t.employeeName || 'Unassigned');
        const dueDate = formatDueDate(t.dueDate);

        let priorityBadge = '<span class="badge bg-warning text-dark">Medium</span>';
        if (t.priority === 'High') {
            priorityBadge = '<span class="badge bg-danger">High</span>';
        } else if (t.priority === 'Low') {
            priorityBadge = '<span class="badge bg-info text-dark">Low</span>';
        }

        let canDrag = true;
        if (userIsMember) {
            canDrag = (t.employeeId === currentEmpId);
        } else if (userIsQC) {
            const qcStages = ['Ready For QC', 'Under QC / To Be Verify', 'QC Failed', 'Done'];
            canDrag = qcStages.includes(normalizeStatus(t.status));
        }

        let actionsHtml = '';
        if (userIsManager) {
            actionsHtml = `
                <button class="btn btn-sm btn-link text-primary p-0 text-decoration-none me-1" onclick="openEditTaskModal(${t.id})" title="Edit Task">
                    <i class="bi bi-pencil-square"></i>
                </button>
                <button class="btn btn-sm btn-link text-danger p-0 text-decoration-none" onclick="promptDeleteTask(${t.id}, '${escapeAttr(t.title)}')" title="Delete Task">
                    <i class="bi bi-trash"></i>
                </button>`;
        } else if (userIsLead) {
            actionsHtml = `
                <button class="btn btn-sm btn-link text-primary p-0 text-decoration-none" onclick="openEditTaskModal(${t.id})" title="Edit Task">
                    <i class="bi bi-pencil-square"></i>
                </button>`;
        } else if (userIsQC) {
            const qcStages = ['Ready For QC', 'Under QC / To Be Verify', 'QC Failed', 'Done'];
            if (qcStages.includes(normalizeStatus(t.status))) {
                actionsHtml = `
                    <button class="btn btn-sm btn-link text-primary p-0 text-decoration-none" onclick="openEditTaskModal(${t.id})" title="Verify / Update QC Status">
                        <i class="bi bi-shield-check"></i>
                    </button>`;
            } else {
                actionsHtml = '';
            }
        } else if (isTeamMember()) {
            if (t.employeeId === currentEmpId) {
                actionsHtml = `
                    <button class="btn btn-sm btn-link text-primary p-0 text-decoration-none" onclick="openEditTaskModal(${t.id})" title="Update Task Status">
                        <i class="bi bi-pencil-square"></i>
                    </button>`;
            } else {
                actionsHtml = '';
            }
        } else {
            actionsHtml = '';
        }

        return `
            <div class="card shadow-sm border bg-white kanban-card mb-2" 
                 draggable="${canDrag}" 
                 data-task-id="${t.id}">
                <div class="card-body p-2">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <span class="badge bg-light text-secondary border">#${t.id}</span>
                        <div class="d-flex align-items-center gap-1">
                            ${priorityBadge}
                            <div class="ms-1">${actionsHtml}</div>
                        </div>
                    </div>
                    <div class="fw-bold text-dark text-truncate mb-1" style="font-size: 0.82rem;" title="${escapeAttr(t.title)}">${safeTitle}</div>
                    <div class="text-muted text-truncate mb-2" style="font-size: 0.72rem;" title="${escapeAttr(t.projectName || '')}">
                        <i class="bi bi-folder me-1"></i>${safeProject}
                    </div>
                    <div class="pt-1 border-top d-flex flex-column gap-1 text-secondary" style="font-size: 0.7rem;">
                        <div class="d-flex align-items-center gap-1 text-truncate" title="${escapeAttr(t.employeeName || 'Unassigned')}">
                            <i class="bi bi-person flex-shrink-0"></i>
                            <span class="text-truncate">${safeEmp}</span>
                        </div>
                        <div class="d-flex align-items-center gap-1 text-nowrap">
                            <i class="bi bi-calendar3 flex-shrink-0"></i>
                            <span>${dueDate}</span>
                        </div>
                    </div>
                </div>
            </div>`;
    }).join('');
}

function bindKanbanDragDropEvents() {
    const $grid = $('#kanbanGrid');

    // Drag start on a task card (jQuery event delegation)
    $grid.on('dragstart', '.kanban-card', function(e) {
        const taskId = $(this).data('task-id');
        const task = allTasks.find(t => t.id === taskId);
        if (!task) return;

        if (isTeamMember() && task.employeeId !== getEmployeeId()) {
            e.preventDefault();
            showToast('Access Denied: You can only update your own assigned tasks.', 'warning');
            return;
        }

        if (isQC()) {
            const qcStages = ['Ready For QC', 'Under QC / To Be Verify', 'QC Failed', 'Done'];
            if (!qcStages.includes(normalizeStatus(task.status))) {
                e.preventDefault();
                showToast('QC can only update tasks in QC stages.', 'warning');
                return;
            }
        }

        draggedTaskId = taskId;
        const dt = e.originalEvent && e.originalEvent.dataTransfer;
        if (dt) {
            dt.setData('text/plain', taskId.toString());
            dt.effectAllowed = 'move';
        }

        const $card = $(this);
        setTimeout(() => $card.addClass('dragging'), 0);
    });

    // Drag end on card
    $grid.on('dragend', '.kanban-card', function() {
        draggedTaskId = null;
        $(this).removeClass('dragging');
        $('.kanban-column-body').removeClass('drag-over');
    });

    // Drag over column body
    $grid.on('dragover', '.kanban-column-body', function(e) {
        e.preventDefault();
        const dt = e.originalEvent && e.originalEvent.dataTransfer;
        if (dt) {
            dt.dropEffect = 'move';
        }
        $(this).addClass('drag-over');
    });

    // Drag leave column body
    $grid.on('dragleave', '.kanban-column-body', function(e) {
        const rect = this.getBoundingClientRect();
        const orig = e.originalEvent || e;
        if (orig.clientX < rect.left || orig.clientX >= rect.right || orig.clientY < rect.top || orig.clientY >= rect.bottom) {
            $(this).removeClass('drag-over');
        }
    });

    // Drop on column body
    $grid.on('drop', '.kanban-column-body', async function(e) {
        e.preventDefault();
        $('.kanban-column-body').removeClass('drag-over');

        const targetStatus = $(this).data('status');
        const dt = e.originalEvent && e.originalEvent.dataTransfer;
        const taskIdStr = (dt ? dt.getData('text/plain') : null) || draggedTaskId;
        const taskId = parseInt(taskIdStr, 10);
        if (!taskId || !targetStatus) return;

        const task = allTasks.find(t => t.id === taskId);
        if (!task) return;

        const currentStageIdx = getStageIndex(task.status);
        const targetStageIdx = getStageIndex(targetStatus);

        if (currentStageIdx === targetStageIdx) {
            return; // Dropped in the same column
        }

        // Validate workflow transition (allows QC Failed -> Under QC / To Be Verify exception)
        if (!isValidStatusTransition(task.status, targetStatus)) {
            showToast(`You cannot move from "${task.status}" to "${targetStatus}".`, 'warning');
            applyTaskFilters();
            return;
        }

        await updateTaskStatus(taskId, targetStatus);
    });
}

async function updateTaskStatus(taskId, newStatus) {
    const task = allTasks.find(t => t.id === taskId);
    if (!task) return;

    if (isTeamMember() && task.employeeId !== getEmployeeId()) {
        showToast('Access Denied: You can only update your own assigned tasks.', 'warning');
        applyTaskFilters();
        return;
    }

    if (isQC()) {
        const qcStages = ['Ready For QC', 'Under QC / To Be Verify', 'QC Failed', 'Done'];
        if (!qcStages.includes(normalizeStatus(task.status)) || !qcStages.includes(normalizeStatus(newStatus))) {
            showToast('QC can only update tasks within QC stages.', 'warning');
            applyTaskFilters();
            return;
        }
    }

    // Validate workflow transition (allows QC Failed -> Under QC / To Be Verify exception)
    if (!isValidStatusTransition(task.status, newStatus)) {
        showToast(`You cannot move from "${task.status}" to "${newStatus}".`, 'warning');
        applyTaskFilters();
        return;
    }

    let formattedDueDate = '';
    if (task.dueDate) {
        try {
            const d = new Date(task.dueDate);
            formattedDueDate = d.toISOString().split('T')[0];
        } catch (_) {
            formattedDueDate = task.dueDate;
        }
    }

    const payload = {
        id: task.id,
        title: task.title,
        description: task.description || '',
        projectId: task.projectId,
        employeeId: task.employeeId,
        priority: task.priority,
        dueDate: formattedDueDate,
        status: newStatus
    };

    try {
        const response = await authFetch(`${TASK_API_URL}/${taskId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            task.status = newStatus;
            showToast(`Task #${task.id} moved to "${newStatus}"!`, 'success');
            applyTaskFilters();
        } else {
            const data = await response.json().catch(() => ({}));
            showToast(data.message || 'Failed to update task status.', 'danger');
            applyTaskFilters();
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
        applyTaskFilters();
    }
}

function openAddTaskModal() {
    if (isTeamMember()) {
        showToast('Access Denied: Team Members cannot create tasks.', 'warning');
        return;
    }

    // Enable inputs
    $('#taskTitle, #taskDesc, #taskProjectId, #taskEmpId, #taskPriority, #taskDueDate, #taskStatus').prop('disabled', false);

    $('#taskId').val('0');
    $('#taskTitle').val('').removeClass('is-invalid');
    $('#taskDesc').val('').removeClass('is-invalid');
    $('#taskProjectId').val('').removeClass('is-invalid');
    
    const $empSelect = $('#taskEmpId');
    if ($empSelect.length) $empSelect.html('<option value="">(Select Employee)</option>').removeClass('is-invalid');

    $('#taskPriority').val('').removeClass('is-invalid');
    $('#taskDueDate').val('').removeClass('is-invalid');
    
    const $statusSelect = $('#taskStatus');
    if ($statusSelect.length) {
        $statusSelect.val('To Do').removeClass('is-invalid');
        $statusSelect.find('option').prop('disabled', false);
    }

    $('#taskModalTitle').text('Add Task');
    $('#btnSaveTask').text('Save');

    const modal = new bootstrap.Modal($('#taskModal')[0]);
    modal.show();
}

async function openEditTaskModal(id) {
    try {
        const response = await authFetch(`${TASK_API_URL}/${id}`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const t = await response.json();

        $('#taskId').val(t.id);
        $('#taskTitle').val(t.title);
        $('#taskDesc').val(t.description || '');
        
        const isMember = isTeamMember();
        const userIsQC = isQC();
        const isRestrictedEdit = isMember || userIsQC;

        $('#taskTitle, #taskDesc').prop('disabled', isRestrictedEdit);

        const $projSelect = $('#taskProjectId');
        if ($projSelect.length) {
            if (t.projectId && !$projSelect.find(`option[value="${t.projectId}"]`).length) {
                $projSelect.append($('<option>', {
                    value: t.projectId,
                    text: t.projectName || 'Assigned Project'
                }));
            }
            $projSelect.val(t.projectId || '').prop('disabled', isRestrictedEdit);
        }

        if (!isRestrictedEdit) {
            onTaskProjectChange();
        }

        const $empSelect = $('#taskEmpId');
        if ($empSelect.length) {
            if (t.employeeId && !$empSelect.find(`option[value="${t.employeeId}"]`).length) {
                $empSelect.append($('<option>', {
                    value: t.employeeId,
                    text: t.employeeName || 'Assigned Member'
                }));
            }
            $empSelect.val(t.employeeId).prop('disabled', isRestrictedEdit);
        }

        $('#taskPriority').val(t.priority).prop('disabled', isRestrictedEdit);
        
        const $dueDateInput = $('#taskDueDate');
        if ($dueDateInput.length) {
            if (t.dueDate) {
                const d = new Date(t.dueDate);
                const dateStr = d.toISOString().split('T')[0];
                $dueDateInput.val(dateStr);
            } else {
                $dueDateInput.val('');
            }
            $dueDateInput.prop('disabled', isRestrictedEdit);
        }

        const $statusSelect = $('#taskStatus');
        if ($statusSelect.length) {
            const normStatus = normalizeStatus(t.status);
            $statusSelect.val(normStatus);

            // In the modal dropdown: disable invalid options based on isValidStatusTransition
            $statusSelect.find('option').each(function() {
                const optVal = $(this).val();
                if (!optVal) return;
                const optNorm = normalizeStatus(optVal);
                if (userIsQC) {
                    const qcStages = ['Ready For QC', 'Under QC / To Be Verify', 'QC Failed', 'Done'];
                    if (!qcStages.includes(optNorm) || !isValidStatusTransition(normStatus, optVal)) {
                        $(this).prop('disabled', true);
                    } else {
                        $(this).prop('disabled', false);
                    }
                } else {
                    if (!isValidStatusTransition(normStatus, optVal)) {
                        $(this).prop('disabled', true);
                    } else {
                        $(this).prop('disabled', false);
                    }
                }
            });
            $statusSelect.prop('disabled', false);
        }

        $('#taskTitle, #taskProjectId, #taskEmpId, #taskPriority, #taskDueDate, #taskStatus').removeClass('is-invalid');

        if (userIsQC) {
            $('#taskModalTitle').text('QC Task Verification & Status');
            $('#btnSaveTask').text('Update QC Status');
        } else if (isMember) {
            $('#taskModalTitle').text('Task Details & Status');
            $('#btnSaveTask').text('Update Status');
        } else {
            $('#taskModalTitle').text('Edit Task');
            $('#btnSaveTask').text('Save');
        }

        const modal = new bootstrap.Modal($('#taskModal')[0]);
        modal.show();
    } catch (err) {
        showToast('Error loading task details: ' + err.message, 'danger');
    }
}

async function saveTask() {
    const id = parseInt($('#taskId').val(), 10) || 0;
    const $titleInput = $('#taskTitle');
    const $descInput = $('#taskDesc');
    const $projInput = $('#taskProjectId');
    const $empInput = $('#taskEmpId');
    const $priorityInput = $('#taskPriority');
    const $dateInput = $('#taskDueDate');
    const $statusInput = $('#taskStatus');
    const $btnSave = $('#btnSaveTask');

    const title = ($titleInput.val() || '').trim();
    const desc = ($descInput.val() || '').trim();
    const projVal = $projInput.val();
    const projectId = projVal ? parseInt(projVal, 10) : null;
    const empId = parseInt($empInput.val(), 10);
    const priority = ($priorityInput.val() || '').trim();
    const dueDate = $dateInput.val();
    const status = ($statusInput.val() || '').trim();

    if (id === 0 && (isTeamMember() || isQC())) {
        showToast('Access Denied: You cannot create tasks.', 'danger');
        return;
    }

    let valid = true;

    if (isTeamMember() || isQC()) {
        // Team member and QC can only change status
        if (!status) {
            $statusInput.addClass('is-invalid');
            $('#taskStatusFeedback').text('Please select a status.');
            valid = false;
        } else {
            if (id > 0) {
                const existing = allTasks.find(t => t.id === id);
                if (existing) {
                    if (isQC()) {
                        const qcStages = ['Ready For QC', 'Under QC / To Be Verify', 'QC Failed', 'Done'];
                        if (!qcStages.includes(normalizeStatus(status))) {
                            $statusInput.addClass('is-invalid');
                            $('#taskStatusFeedback').text('QC role can only set QC workflow statuses.');
                            valid = false;
                        }
                    }
                    if (valid && !isValidStatusTransition(existing.status, status)) {
                        $statusInput.addClass('is-invalid');
                        $('#taskStatusFeedback').text(`You cannot move from "${existing.status}" to "${status}".`);
                        valid = false;
                    }
                }
            }
            if (valid) $statusInput.removeClass('is-invalid');
        }
    } else {
        if (!title) {
            $titleInput.addClass('is-invalid');
            $('#taskTitleFeedback').text('Title is required.');
            valid = false;
        } else if (title.length > 200) {
            $titleInput.addClass('is-invalid');
            $('#taskTitleFeedback').text('Title cannot exceed 200 characters.');
            valid = false;
        } else {
            $titleInput.removeClass('is-invalid');
        }

        if (desc.length > 500) {
            $descInput.addClass('is-invalid');
            $('#taskDescFeedback').text('Description cannot exceed 500 characters.');
            valid = false;
        } else {
            $descInput.removeClass('is-invalid');
        }

        if (!projectId) {
            $projInput.addClass('is-invalid');
            $('#taskProjectFeedback').text('Please select a project.');
            valid = false;
        } else {
            $projInput.removeClass('is-invalid');
        }

        if (!empId) {
            $empInput.addClass('is-invalid');
            $('#taskEmpFeedback').text('Please select an employee.');
            valid = false;
        } else {
            $empInput.removeClass('is-invalid');
        }

        if (!priority) {
            $priorityInput.addClass('is-invalid');
            $('#taskPriorityFeedback').text('Please select a priority.');
            valid = false;
        } else {
            $priorityInput.removeClass('is-invalid');
        }

        if (!dueDate) {
            $dateInput.addClass('is-invalid');
            $('#taskDueDateFeedback').text('Due date is required.');
            valid = false;
        } else {
            $dateInput.removeClass('is-invalid');
        }

        if (!status) {
            $statusInput.addClass('is-invalid');
            $('#taskStatusFeedback').text('Please select a status.');
            valid = false;
        } else {
            // Verify valid status transition (except QC Failed -> Under QC exception)
            if (id > 0) {
                const existing = allTasks.find(t => t.id === id);
                if (existing) {
                    if (!isValidStatusTransition(existing.status, status)) {
                        $statusInput.addClass('is-invalid');
                        $('#taskStatusFeedback').text(`You cannot move from "${existing.status}" to "${status}".`);
                        valid = false;
                    }
                }
            }
            if (valid) $statusInput.removeClass('is-invalid');
        }
    }

    if (!valid) return;

    $btnSave.prop('disabled', true).text('Saving...');

    const isEdit = id > 0;
    const url = isEdit ? `${TASK_API_URL}/${id}` : TASK_API_URL;
    const method = isEdit ? 'PUT' : 'POST';

    try {
        const response = await authFetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                id: id,
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

        if (response.status === 201 || response.status === 200) {
            const modal = bootstrap.Modal.getInstance($('#taskModal')[0]);
            if (modal) modal.hide();

            showToast(isEdit ? 'Task updated successfully!' : 'Task created successfully!', 'success');
            loadTasks();
        } else {
            showToast(data.message || 'Failed to save task.', 'danger');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
    } finally {
        $btnSave.prop('disabled', false).text('Save');
    }
}

function promptDeleteTask(id, title) {
    if (!isManager()) {
        showToast('Access Denied: Only Managers can delete tasks.', 'warning');
        return;
    }

    pendingDeleteTaskId = id;
    $('#deleteTaskTitleText').text(`"${title}"`);
    const modal = new bootstrap.Modal($('#deleteTaskModal')[0]);
    modal.show();
}

async function confirmDeleteTask() {
    if (!isManager()) {
        showToast('Access Denied: Only Managers can delete tasks.', 'danger');
        return;
    }

    if (!pendingDeleteTaskId) return;

    try {
        const response = await authFetch(`${TASK_API_URL}/${pendingDeleteTaskId}`, { method: 'DELETE' });
        const modal = bootstrap.Modal.getInstance($('#deleteTaskModal')[0]);
        if (modal) modal.hide();

        if (response.status === 200) {
            showToast('Task deleted successfully!', 'success');
            loadTasks();
        } else {
            const data = await response.json().catch(() => ({}));
            showToast(data.message || 'Failed to delete task.', 'danger');
        }
    } catch (err) {
        showToast('Error: ' + err.message, 'danger');
    } finally {
        pendingDeleteTaskId = 0;
    }
}

function formatDueDate(isoStr) {
    if (!isoStr) return '';
    try {
        const d = new Date(isoStr);
        if (isNaN(d.getTime())) return isoStr;
        const day = String(d.getDate()).padStart(2, '0');
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const year = d.getFullYear();
        return `${day}/${month}/${year}`;
    } catch (_) {
        return isoStr;
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
