// =======================================================================
// Home Dashboard JavaScript - Dynamic Tile Loading by Role
// =======================================================================

$(document).ready(async function () {
    const role = getUserRole();

    if (isManager()) {
        // Manager sees all 4 modules
        authFetch('/api/department')
            .then(r => r && r.ok ? r.json() : [])
            .then(d => { $('#dashDeptCount').text(d.length || 0); })
            .catch(() => {});

        authFetch('/api/employee')
            .then(r => r && r.ok ? r.json() : [])
            .then(e => { $('#dashEmpCount').text(e.length || 0); })
            .catch(() => {});

        authFetch('/api/project')
            .then(r => r && r.ok ? r.json() : [])
            .then(p => { $('#dashProjCount').text(p.length || 0); })
            .catch(() => {});

        authFetch('/api/task')
            .then(r => r && r.ok ? r.json() : [])
            .then(t => { $('#dashTaskCount').text(t.length || 0); })
            .catch(() => {});
    } else if (isTeamLead()) {
        // Team Lead sees only Projects & Tasks
        $('#cardDashDept').hide();
        $('#cardDashEmp').hide();
        $('#dashProjTitle').text('MY PROJECTS');
        $('#dashProjLinkText').text('View My Projects');
        $('#dashTaskTitle').text('PROJECT TASKS');
        $('#dashTaskLinkText').text('View Project Tasks');

        authFetch('/api/project')
            .then(r => r && r.ok ? r.json() : [])
            .then(p => { $('#dashProjCount').text(p.length || 0); })
            .catch(() => {});

        authFetch('/api/task')
            .then(r => r && r.ok ? r.json() : [])
            .then(t => { $('#dashTaskCount').text(t.length || 0); })
            .catch(() => {});
    } else {
        // Developer / QC sees only Tasks
        $('#cardDashDept').hide();
        $('#cardDashEmp').hide();
        $('#cardDashProj').hide();
        $('#dashTaskTitle').text(isQC() ? 'QC VERIFICATION TASKS' : 'MY TASKS');
        $('#dashTaskLinkText').text(isQC() ? 'View QC Tasks' : 'View My Tasks');

        authFetch('/api/task')
            .then(r => r && r.ok ? r.json() : [])
            .then(t => { $('#dashTaskCount').text(t.length || 0); })
            .catch(() => {});
    }
});
