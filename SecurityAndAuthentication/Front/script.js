// Configuração da API
const API_BASE_URL = 'http://localhost:5150/api'; // Ajuste a porta conforme necessário

// Validação de confirmação de senha
function validatePasswordConfirmation() {
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const confirmInput = document.getElementById('confirmPassword');
    
    if (password !== confirmPassword && confirmPassword !== '') {
        confirmInput.style.borderColor = '#e74c3c';
        return false;
    } else if (password === confirmPassword && confirmPassword !== '') {
        confirmInput.style.borderColor = '#1976d2';
        return true;
    }
    return false;
}

// Função para registrar usuário
async function registerUser(userData) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData)
        });

        const result = await response.json();

        if (response.ok) {
            return { success: true, data: result };
        } else {
            return { success: false, error: result.message || 'Registration failed' };
        }
    } catch (error) {
        console.error('Error:', error);
        return { success: false, error: 'Network error occurred' };
    }
}

// Event listener para o formulário
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('registrationForm');
    const inputs = document.querySelectorAll('input[type="text"], input[type="email"], input[type="password"]');
    
    // Adicionar interatividade ao formulário
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Verificar se as senhas coincidem
        if (!validatePasswordConfirmation()) {
            alert('Passwords do not match!');
            return;
        }
        
        // Coletar dados do formulário
        const formData = new FormData(this);
        const userData = {
            email: formData.get('email'),
            username: formData.get('username'),
            password: formData.get('password'),
            confirmPassword: formData.get('confirmPassword'),
            subscribe: document.getElementById('subscribe').checked
        };
        
        // Simular processamento
        const button = this.querySelector('button');
        const originalText = button.textContent;
        
        button.textContent = 'Creating...';
        button.disabled = true;
        
        try {
            const result = await registerUser(userData);
            
            if (result.success) {
                button.textContent = 'Account created!';
                button.style.background = '#4caf50';
                
                alert(`Welcome, ${result.data.username}! Your account has been created successfully.`);
                
                setTimeout(() => {
                    // Redirecionar para login após criar conta
                    window.location.href = 'login.html';
                }, 2000);
            } else {
                button.textContent = 'Error occurred';
                button.style.background = '#f44336';
                
                alert(`Registration failed: ${result.error}`);
                
                setTimeout(() => {
                    button.textContent = originalText;
                    button.disabled = false;
                    button.style.background = '#1976d2';
                }, 2000);
            }
        } catch (error) {
            button.textContent = 'Network error';
            button.style.background = '#f44336';
            
            alert('Network error occurred. Please try again.');
            
            setTimeout(() => {
                button.textContent = originalText;
                button.disabled = false;
                button.style.background = '#1976d2';
            }, 2000);
        }
    });
    
    // Validação em tempo real
    inputs.forEach(input => {
        input.addEventListener('input', function() {
            // Validação especial para confirmação de senha
            if (this.id === 'confirmPassword' || this.id === 'password') {
                validatePasswordConfirmation();
            }
        });
        
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
});
