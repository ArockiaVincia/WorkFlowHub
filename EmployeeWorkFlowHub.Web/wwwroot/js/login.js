// =======================================================================
// Login Page JavaScript
// =======================================================================

// Clear any stale local storage tokens when arriving at the login page
$(document).ready(function () {
    clearAuthSession();
});

// Toggle password visibility
$('#btnTogglePassword').on('click', function () {
    const passInput = $('#txtPassword');
    const icon = $('#toggleIcon');
    if (passInput.attr('type') === 'password') {
        passInput.attr('type', 'text');
        icon.attr('class', 'bi bi-eye-slash');
    } else {
        passInput.attr('type', 'password');
        icon.attr('class', 'bi bi-eye');
    }
});

function showAlert(message) {
    $('#loginAlertText').text(message);
    $('#loginAlert').removeAttr('style').show();
}

function hideAlert() {
    $('#loginAlert').attr('style', 'display: none !important;').hide();
}

// Form Submission
$('#loginForm').on('submit', async function (e) {
    e.preventDefault();
    hideAlert();

    const username = $('#txtUsername').val() ? $('#txtUsername').val().trim() : '';
    const password = $('#txtPassword').val() || '';

    if (!username) {
        showAlert('Username is required.');
        $('#txtUsername').trigger('focus');
        return;
    }

    if (!password) {
        showAlert('Password is required.');
        $('#txtPassword').trigger('focus');
        return;
    }

    // Spinner state
    const btnLogin = $('#btnLogin');
    const spinner = $('#loginSpinner');
    const icon = $('#loginIcon');
    const btnText = $('#loginBtnText');

    btnLogin.prop('disabled', true);
    spinner.removeClass('d-none');
    icon.addClass('d-none');
    btnText.text('Authenticating...');

    try {
        const response = await fetch(getApiUrl('/api/auth/login'), {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                password: password
            })
        });

        if (response.ok) {
            const data = await response.json();
            setAuthSession(data);
            window.location.href = '/';
        } else {
            let errMsg = 'Invalid username or password.';
            try {
                const errData = await response.json();
                if (errData && errData.message) {
                    errMsg = errData.message;
                }
            } catch (_) {}
            showAlert(errMsg);
        }
    } catch (err) {
        showAlert('Unable to reach authentication service. Please ensure the server is running.');
    } finally {
        btnLogin.prop('disabled', false);
        spinner.addClass('d-none');
        icon.removeClass('d-none');
        btnText.text('Sign In');
    }
});
