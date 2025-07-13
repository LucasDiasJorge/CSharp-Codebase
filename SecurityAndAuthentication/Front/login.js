// Configuração da API
const API_BASE_URL = 'http://localhost:5150/api'; // Ajuste a porta conforme necessário

// Classe para gerenciar autenticação
class AuthManager {
    constructor() {
        this.token = localStorage.getItem('authToken');
        this.user = JSON.parse(localStorage.getItem('userData') || 'null');
    }

    // Fazer login
    async login(credentials) {
        try {
            const response = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(credentials)
            });

            const result = await response.json();

            if (response.ok) {
                // Usar token JWT real retornado pelo backend
                const token = result.token;
                
                // Salvar dados de autenticação
                localStorage.setItem('authToken', token);
                localStorage.setItem('userData', JSON.stringify(result));
                
                this.token = token;
                this.user = result;
                
                return { success: true, data: result };
            } else {
                return { success: false, error: result.message || 'Login failed' };
            }
        } catch (error) {
            console.error('Login error:', error);
            return { success: false, error: 'Network error occurred' };
        }
    }

    // Fazer logout
    logout() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('userData');
        this.token = null;
        this.user = null;
        
        // Redirecionar para login
        window.location.href = 'login.html';
    }

    // Verificar se está autenticado
    isAuthenticated() {
        return this.token && this.user && !this.isTokenExpired();
    }

    // Verificar se o token está expirado (simples check no frontend)
    isTokenExpired() {
        if (!this.token) return true;
        
        try {
            // Decodificar JWT no frontend (apenas para verificar expiração)
            const payload = JSON.parse(atob(this.token.split('.')[1]));
            const currentTime = Math.floor(Date.now() / 1000);
            return payload.exp < currentTime;
        } catch (error) {
            console.error('Error checking token expiration:', error);
            return true;
        }
    }

    // Validar token com o backend
    async validateToken() {
        if (!this.token) return false;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/validate-token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ token: this.token })
            });

            const result = await response.json();
            
            if (response.ok && result.valid) {
                // Atualizar dados do usuário se necessário
                if (result.user) {
                    this.user = { ...this.user, ...result.user };
                    localStorage.setItem('userData', JSON.stringify(this.user));
                }
                return true;
            } else {
                // Token inválido, fazer logout
                this.logout();
                return false;
            }
        } catch (error) {
            console.error('Token validation error:', error);
            return false;
        }
    }

    // Renovar token
    async refreshToken() {
        if (!this.token) return false;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/refresh-token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ token: this.token })
            });

            const result = await response.json();

            if (response.ok && result.token) {
                // Atualizar token e dados
                this.token = result.token;
                this.user = { ...this.user, ...result };
                
                localStorage.setItem('authToken', this.token);
                localStorage.setItem('userData', JSON.stringify(this.user));
                
                return true;
            } else {
                this.logout();
                return false;
            }
        } catch (error) {
            console.error('Token refresh error:', error);
            this.logout();
            return false;
        }
    }

    // Obter dados do usuário
    getUser() {
        return this.user;
    }

    // Buscar dados atualizados do usuário
    async fetchUserData(userId) {
        try {
            const response = await fetch(`${API_BASE_URL}/auth/users/${userId}`, {
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            if (response.ok) {
                const userData = await response.json();
                localStorage.setItem('userData', JSON.stringify({...this.user, ...userData}));
                this.user = {...this.user, ...userData};
                return userData;
            }
        } catch (error) {
            console.error('Error fetching user data:', error);
        }
        return null;
    }
}

// Instância global do gerenciador de autenticação
const authManager = new AuthManager();

// Função para mostrar mensagens
function showMessage(message, type = 'info') {
    // Remove mensagem anterior se existir
    const existingMessage = document.querySelector('.message');
    if (existingMessage) {
        existingMessage.remove();
    }

    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${type}`;
    messageDiv.textContent = message;

    const form = document.getElementById('loginForm');
    form.parentNode.insertBefore(messageDiv, form);

    // Remove a mensagem após 5 segundos
    setTimeout(() => {
        if (messageDiv.parentNode) {
            messageDiv.remove();
        }
    }, 5000);
}

// Função para exibir informações do usuário
function displayUserInfo(userData) {
    const userInfoDiv = document.getElementById('userInfo');
    const userDataDiv = document.getElementById('userData');
    const loginForm = document.getElementById('loginForm');

    // Esconder formulário de login
    loginForm.classList.add('hidden');

    // Mostrar informações do usuário
    userDataDiv.innerHTML = `
        <div class="user-details">
            <p><strong>User ID:</strong> ${userData.userId}</p>
            <p><strong>Username:</strong> ${userData.username}</p>
            <p><strong>Email:</strong> ${userData.email}</p>
            <p><strong>Login Time:</strong> ${new Date().toLocaleString()}</p>
        </div>
    `;

    userInfoDiv.classList.remove('hidden');
}

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    const inputs = document.querySelectorAll('input[type="text"], input[type="password"]');

    // Verificar se já está logado
    if (authManager.isAuthenticated()) {
        displayUserInfo(authManager.getUser());
        showMessage('You are already logged in!', 'success');
    }

    // Manipular envio do formulário de login
    loginForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        const formData = new FormData(this);
        const credentials = {
            emailOrUsername: formData.get('emailOrUsername'),
            password: formData.get('password')
        };

        const button = this.querySelector('button');
        const originalText = button.textContent;

        // Estado de carregamento
        button.textContent = 'Signing in...';
        button.disabled = true;
        this.classList.add('loading');

        try {
            const result = await authManager.login(credentials);

            if (result.success) {
                showMessage('Login successful! Redirecting to dashboard...', 'success');
                
                setTimeout(() => {
                    window.location.href = 'dashboard.html';
                }, 1500);

            } else {
                showMessage(`Login failed: ${result.error}`, 'error');
            }
        } catch (error) {
            showMessage('Network error occurred. Please try again.', 'error');
        } finally {
            // Restaurar estado do botão
            button.textContent = originalText;
            button.disabled = false;
            this.classList.remove('loading');
        }
    });

    // Validação visual dos campos
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.style.borderColor = '#1976d2';
        });

        input.addEventListener('blur', function() {
            if (this.value === '') {
                this.style.borderColor = '#ddd';
            } else if (this.checkValidity()) {
                this.style.borderColor = '#ddd';
            } else {
                this.style.borderColor = '#e74c3c';
            }
        });
    });

    // Manipular logout
    document.getElementById('btnLogout').addEventListener('click', function() {
        if (confirm('Are you sure you want to logout?')) {
            authManager.logout();
        }
    });
});

// Função para verificar autenticação em outras páginas
async function requireAuth() {
    if (!authManager.isAuthenticated()) {
        window.location.href = 'login.html';
        return false;
    }
    
    // Validar token com o backend
    const isValid = await authManager.validateToken();
    if (!isValid) {
        return false; // Já fez logout na validação
    }
    
    return true;
}

// Exportar para uso global
window.authManager = authManager;
window.requireAuth = requireAuth;
