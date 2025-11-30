class Auth {
    static async login(email, password) {
        const formData = new FormData();
        formData.append('email', email);
        formData.append('password', password);

        const response = await fetch('/api/auth/login', {
            method: 'POST',
            body: formData
        });
        return await response.json();
    }

    static async register(firstName, lastName, email, password) {
        const formData = new FormData();
        formData.append('firstName', firstName);
        formData.append('lastName', lastName);
        formData.append('email', email);
        formData.append('password', password);

        const response = await fetch('/api/auth/register', {
            method: 'POST',
            body: formData
        });
        return await response.json();
    }

    static async checkAuth() {
        const response = await fetch('/api/auth/current');
        return await response.json();
    }

    static async logout() {
        await fetch('/logout', { method: 'GET' });
        window.location.href = '/login.html';
    }
}

if (window.location.pathname === '/' || window.location.pathname === '/index.html') {
    document.addEventListener('DOMContentLoaded', async () => {
        const data = await Auth.checkAuth();
        const loginLink = document.getElementById('loginLink');
        const registerLink = document.getElementById('registerLink');
        const logoutBtn = document.getElementById('logoutBtn');
        const userWelcome = document.getElementById('userWelcome');

        if (data.isAuthenticated) {
            if (loginLink) loginLink.classList.add('d-none');
            if (registerLink) registerLink.classList.add('d-none');
            if (logoutBtn) logoutBtn.classList.remove('d-none');
            if (userWelcome) {
                userWelcome.classList.remove('d-none');
                userWelcome.textContent = 'Witaj!';
            }
        } else {
            if (loginLink) loginLink.classList.remove('d-none');
            if (registerLink) registerLink.classList.remove('d-none');
            if (logoutBtn) logoutBtn.classList.add('d-none');
            if (userWelcome) userWelcome.classList.add('d-none');
        }

        if (logoutBtn) {
            logoutBtn.addEventListener('click', (e) => {
                e.preventDefault();
                Auth.logout();
            });
        }
    });
} else {
    document.addEventListener('DOMContentLoaded', async () => {
        const data = await Auth.checkAuth();
        const loginLink = document.getElementById('loginLink');
        const registerLink = document.getElementById('registerLink');
        const logoutBtn = document.getElementById('logoutBtn');
        const userWelcome = document.getElementById('userWelcome');

        if (data.isAuthenticated) {
            if (loginLink) loginLink.classList.add('d-none');
            if (registerLink) registerLink.classList.add('d-none');
            if (logoutBtn) logoutBtn.classList.remove('d-none');
            if (userWelcome) userWelcome.classList.add('d-none');
        } else {
            if (loginLink) loginLink.classList.remove('d-none');
            if (registerLink) registerLink.classList.remove('d-none');
            if (logoutBtn) logoutBtn.classList.add('d-none');
            if (userWelcome) userWelcome.classList.add('d-none');
        }

        if (logoutBtn) {
            logoutBtn.addEventListener('click', (e) => {
                e.preventDefault();
                Auth.logout();
            });
        }
    });
}