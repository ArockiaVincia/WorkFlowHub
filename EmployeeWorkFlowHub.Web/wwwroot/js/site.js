// =======================================================================
// WorkFlow Hub - Global Auth & UI Utilities
// =======================================================================

const AUTH_KEYS = {
    TOKEN: 'wf_token',
    USERNAME: 'wf_username',
    FULL_NAME: 'wf_fullname',
    ROLE: 'wf_role',
    EMPLOYEE_ID: 'wf_employee_id'
};

function getToken() {
    const val = $('#hdtoken').val();
    if (val && val.trim().length > 0) return val.trim();
    return localStorage.getItem(AUTH_KEYS.TOKEN) || '';
}

function getUsername() {
    const val = $('#hdLoginUsername').val();
    if (val && val.trim().length > 0) return val.trim();
    return localStorage.getItem(AUTH_KEYS.USERNAME) || '';
}

function getFullName() {
    const val = $('#hdLoginFullName').val();
    if (val && val.trim().length > 0) return val.trim();
    return localStorage.getItem(AUTH_KEYS.FULL_NAME) || '';
}

function getUserRole() {
    const val = $('#hdUserRole').val();
    if (val && val.trim().length > 0) return val.trim();
    return localStorage.getItem(AUTH_KEYS.ROLE) || '';
}

function getEmployeeId() {
    const val = $('#hdEmployeeId').val();
    if (val && val.trim().length > 0) return parseInt(val, 10) || 0;
    return parseInt(localStorage.getItem(AUTH_KEYS.EMPLOYEE_ID), 10) || 0;
}

function isAuthenticated() {
    const token = getToken();
    return !!token && token.trim().length > 0;
}

function isManager() {
    const role = (getUserRole() || '').toLowerCase();
    return role === 'manager' || role === 'admin';
}

function isAdmin() {
    return isManager();
}

function isTeamLead() {
    return (getUserRole() || '').toLowerCase().includes('lead');
}

function isTeamMember() {
    const role = (getUserRole() || '').toLowerCase();
    return role.includes('member') || role.includes('developer') || role === 'user';
}

function isQC() {
    const role = (getUserRole() || '').toLowerCase();
    return role.includes('qc') || role.includes('quality');
}

function setAuthSession(authData) {
    if (!authData) return;
    if (authData.token) localStorage.setItem(AUTH_KEYS.TOKEN, authData.token);
    if (authData.username) localStorage.setItem(AUTH_KEYS.USERNAME, authData.username);
    if (authData.fullName) localStorage.setItem(AUTH_KEYS.FULL_NAME, authData.fullName);
    if (authData.role) localStorage.setItem(AUTH_KEYS.ROLE, authData.role);
    if (authData.employeeId !== undefined && authData.employeeId !== null) {
        localStorage.setItem(AUTH_KEYS.EMPLOYEE_ID, authData.employeeId.toString());
    }
}

function clearAuthSession() {
    localStorage.removeItem(AUTH_KEYS.TOKEN);
    localStorage.removeItem(AUTH_KEYS.USERNAME);
    localStorage.removeItem(AUTH_KEYS.FULL_NAME);
    localStorage.removeItem(AUTH_KEYS.ROLE);
    localStorage.removeItem(AUTH_KEYS.EMPLOYEE_ID);
}

function logout() {
    clearAuthSession();
    window.location.href = '/Account/Logout';
}

// Resolves relative API paths to the configured Web API Base URL
function getApiUrl(path) {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    const base = ($('#hdnAPIBaseURL').val() || 'http://localhost:5001').replace(/\/+$/, '');
    return base + (path.startsWith('/') ? path : '/' + path);
}

// Authenticated fetch wrapper injecting Bearer token and handling 401/403
async function authFetch(url, options = {}) {
    const fullUrl = getApiUrl(url);
    const token = getToken();
    options.headers = options.headers || {};

    if (token) {
        if (options.headers instanceof Headers) {
            options.headers.set('Authorization', 'Bearer ' + token);
        } else {
            options.headers['Authorization'] = 'Bearer ' + token;
        }
    }

    try {
        const response = await fetch(fullUrl, options);

        if (response.status === 401) {
            // Token expired or invalid credentials
            clearAuthSession();
            if (!window.location.pathname.toLowerCase().includes('/account/login')) {
                window.location.href = '/Account/Login';
            }
            return response;
        }

        if (response.status === 403) {
            showToast('Access Denied: You do not have permission to perform this action.', 'danger');
        }

        return response;
    } catch (error) {
        console.error('Fetch error on URL: ' + url, error);
        throw error;
    }
}

// Global Toast Notification Helper
function showToast(message, type = 'success') {
    const $toastEl = $('#appToast');
    const $toastText = $('#toastText');
    const $toastIcon = $('#toastIcon');

    if ($toastEl.length === 0 || $toastText.length === 0) return;

    // Set styling based on type
    $toastEl.attr('class', 'toast align-items-center text-white border-0 shadow');
    if (type === 'success') {
        $toastEl.addClass('bg-success');
        if ($toastIcon.length) $toastIcon.attr('class', 'bi bi-check-circle fs-5');
    } else if (type === 'warning') {
        $toastEl.addClass('bg-warning text-dark');
        if ($toastIcon.length) $toastIcon.attr('class', 'bi bi-exclamation-triangle fs-5 text-dark');
    } else if (type === 'danger') {
        $toastEl.addClass('bg-danger');
        if ($toastIcon.length) $toastIcon.attr('class', 'bi bi-x-circle fs-5');
    } else {
        $toastEl.addClass('bg-primary');
        if ($toastIcon.length) $toastIcon.attr('class', 'bi bi-info-circle fs-5');
    }

    $toastText.text(message);

    const toast = new bootstrap.Toast($toastEl[0], { delay: 4000 });
    toast.show();
}

// Update Layout Navbar on page load based on current user session
function updateNavAuthDisplay() {
    const isLoginPath = window.location.pathname.toLowerCase().includes('/account/login');
    const authenticated = isAuthenticated();

    // Route guard for view shells
    if (!authenticated && !isLoginPath) {
        window.location.href = '/Account/Login';
        return;
    }

    const $userRoleEl = $('#navUserRole, #navUserBadge');
    const $usernameEl = $('#navUsername');
    const $logoutBtnEl = $('#navLogoutBtn');
    const $navItemOrg = $('#navItemOrg');
    const $navItemProjects = $('#navItemProjects');
    const $navItemTasks = $('#navItemTasks');
    const $navTextProjects = $('#navTextProjects');
    const $navTextTasks = $('#navTextTasks');

    if (authenticated) {
        const displayName = getUsername() || 'User';
        $usernameEl.text(displayName);
        $userRoleEl.hide();
        $logoutBtnEl.css('display', 'inline-flex');

        // Role-based Navbar visibility
        if (isManager()) {
            $navItemOrg.show();
            $navItemProjects.show();
            $navItemTasks.show();
            $navTextProjects.text('Projects');
            $navTextTasks.text('Tasks');
        } else if (isTeamLead()) {
            $navItemOrg.hide();
            $navItemProjects.show();
            $navItemTasks.show();
            $navTextProjects.text('My Projects');
            $navTextTasks.text('My Project Tasks');
        } else if (isQC()) {
            $navItemOrg.hide();
            $navItemProjects.hide();
            $navItemTasks.show();
            $navTextTasks.text('QC Verification Tasks');
        } else {
            // Developer / Team Member
            $navItemOrg.hide();
            $navItemProjects.hide();
            $navItemTasks.show();
            $navTextTasks.text('My Tasks');
        }
    } else {
        $userRoleEl.hide();
        $usernameEl.text('');
        $logoutBtnEl.hide();
    }
}

// Global Support Data Dropdown Loader (Database-driven options)
async function fetchLookupItems(lookupType) {
    try {
        const response = await authFetch(`/api/supportdata/${encodeURIComponent(lookupType)}`);
        if (response && response.ok) {
            return await response.json();
        }
        return [];
    } catch (err) {
        console.error(`Error fetching support data items for '${lookupType}':`, err);
        return [];
    }
}

async function populateLookupDropdown($select, lookupType, defaultText = '(Select)', selectedVal = '') {
    if (!$select || !$select.length) return [];
    try {
        const items = await fetchLookupItems(lookupType);
        $select.empty();
        if (defaultText) {
            $select.append($('<option>', { value: '', text: defaultText }));
        }
        items.forEach(item => {
            const val = item.value || item.lookupValue || '';
            const text = item.value || item.lookupValue || item.code || '';
            const $opt = $('<option>', { value: val, text: text });
            if (selectedVal && selectedVal === val) {
                $opt.prop('selected', true);
            }
            $select.append($opt);
        });
        if (selectedVal) {
            $select.val(selectedVal);
        }
        return items;
    } catch (err) {
        console.error(`Error populating dropdown for '${lookupType}':`, err);
        return [];
    }
}

$(document).ready(function() {
    updateNavAuthDisplay();
});

