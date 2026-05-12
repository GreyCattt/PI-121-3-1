/**
 * Internet Auction UI
 * JavaScript логіка для пошуку й фільтрації лотів
 */

// API базовий URL
const API_BASE_URL = '/api';
const TOKEN_STORAGE_KEY = 'auction.jwt';

// DOM елементи
const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');
const meButton = document.getElementById('meButton');
const logoutButton = document.getElementById('logoutButton');
const authMessage = document.getElementById('authMessage');
const tokenOutput = document.getElementById('tokenOutput');
const currentUserOutput = document.getElementById('currentUserOutput');
const authStatusBadge = document.getElementById('authStatusBadge');
const searchForm = document.getElementById('searchForm');
const lotsList = document.getElementById('lotsList');
const loadingElement = document.getElementById('loading');
const errorElement = document.getElementById('error');
const noResultsElement = document.getElementById('noResults');
const modal = document.getElementById('lotModal');
const closeBtn = document.querySelector('.close');

// Слухачі подій
loginForm.addEventListener('submit', handleLogin);
registerForm.addEventListener('submit', handleRegister);
meButton.addEventListener('click', loadCurrentUser);
logoutButton.addEventListener('click', logout);
searchForm.addEventListener('submit', handleSearch);
closeBtn.addEventListener('click', closeModal);
window.addEventListener('click', (e) => {
    if (e.target === modal) closeModal();
});

initializeAuthState();

/**
 * Обробник форми пошуку
 */
async function handleSearch(e) {
    e.preventDefault();
    
    // Отримуємо значення з форми
    const searchQuery = document.getElementById('searchQuery').value || null;
    const minPrice = document.getElementById('minPrice').value || null;
    const maxPrice = document.getElementById('maxPrice').value || null;
    const status = document.getElementById('status').value || null;
    const categoryId = document.getElementById('categoryId').value || null;

    // Приховуємо повідомлення про відсутність результатів
    noResultsElement.style.display = 'none';
    errorElement.style.display = 'none';

    // Показуємо статус завантаження
    loadingElement.style.display = 'block';
    lotsList.innerHTML = '';

    try {
        // Будуємо URL з параметрами
        const params = new URLSearchParams();
        if (searchQuery) params.append('searchQuery', searchQuery);
        if (minPrice) params.append('minPrice', minPrice);
        if (maxPrice) params.append('maxPrice', maxPrice);
        if (status) params.append('status', status);
        if (categoryId) params.append('categoryId', categoryId);

        // Зробимо запит до API
        const response = await fetch(`${API_BASE_URL}/lots/search?${params}`);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const lots = await response.json();

        // Приховуємо статус завантаження
        loadingElement.style.display = 'none';

        if (lots.length === 0) {
            noResultsElement.style.display = 'block';
            return;
        }

        // Відображаємо лоти
        displayLots(lots);

    } catch (error) {
        loadingElement.style.display = 'none';
        errorElement.textContent = `❌ Помилка: ${error.message}`;
        errorElement.style.display = 'block';
        console.error('Помилка пошуку:', error);
    }
}

async function handleLogin(e) {
    e.preventDefault();

    const payload = {
        email: document.getElementById('loginEmail').value,
        password: document.getElementById('loginPassword').value
    };

    await authenticate('/auth/login', payload, 'Вхід виконано успішно');
}

async function handleRegister(e) {
    e.preventDefault();

    const payload = {
        username: document.getElementById('registerUsername').value,
        email: document.getElementById('registerEmail').value,
        password: document.getElementById('registerPassword').value
    };

    await authenticate('/auth/register', payload, 'Реєстрацію завершено успішно');
}

async function authenticate(endpoint, payload, successMessage) {
    showAuthMessage('');

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data?.error || 'Помилка авторизації');
        }

        const token = data.token || data.Token;
        if (!token) {
            throw new Error('Сервер не повернув JWT token');
        }

        setToken(token);
        showAuthMessage(successMessage);
        await loadCurrentUser();
    } catch (error) {
        showAuthMessage(`❌ ${error.message}`, true);
    }
}

async function loadCurrentUser() {
    const token = getToken();
    if (!token) {
        currentUserOutput.textContent = 'Користувач не авторизований';
        updateAuthBadge(false);
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/auth/me`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data?.error || 'Не вдалося отримати профіль');
        }

        currentUserOutput.textContent = JSON.stringify(data, null, 2);
        updateAuthBadge(true, data.username);
    } catch (error) {
        currentUserOutput.textContent = `❌ ${error.message}`;
        updateAuthBadge(false);
    }
}

function initializeAuthState() {
    const token = getToken();
    if (token) {
        tokenOutput.value = token;
        loadCurrentUser();
    } else {
        tokenOutput.value = '';
        currentUserOutput.textContent = 'Користувач не авторизований';
        updateAuthBadge(false);
    }
}

function logout() {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    tokenOutput.value = '';
    currentUserOutput.textContent = 'Користувач не авторизований';
    showAuthMessage('Вихід виконано');
    updateAuthBadge(false);
}

function setToken(token) {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
    tokenOutput.value = token;
}

function getToken() {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
}

function updateAuthBadge(isAuthenticated, username = '') {
    authStatusBadge.className = `status-pill ${isAuthenticated ? 'status-authenticated' : 'status-guest'}`;
    authStatusBadge.textContent = isAuthenticated ? `Auth${username ? `: ${username}` : ''}` : 'Guest';
}

function showAuthMessage(message, isError = false) {
    if (!message) {
        authMessage.style.display = 'none';
        authMessage.textContent = '';
        authMessage.style.borderLeftColor = '#667eea';
        return;
    }

    authMessage.textContent = message;
    authMessage.style.display = 'block';
    authMessage.style.borderLeftColor = isError ? '#c33' : '#667eea';
    authMessage.style.color = isError ? '#8d1f1f' : '#243b73';
    authMessage.style.background = isError ? '#fff1f1' : '#eef4ff';
}

/**
 * Відображення лотів в grid
 */
function displayLots(lots) {
    lotsList.innerHTML = '';

    lots.forEach(lot => {
        const lotCard = createLotCard(lot);
        lotsList.appendChild(lotCard);
    });
}

/**
 * Створення картки лота
 */
function createLotCard(lot) {
    const card = document.createElement('div');
    card.className = 'lot-card';
    card.onclick = () => showLotDetails(lot);

    // Форматування статусу
    const statusClass = `status-${lot.status.toLowerCase()}`;
    const statusLabel = formatStatus(lot.status);

    // Форматування цін
    const startingPrice = formatCurrency(lot.startingPrice);
    const currentPrice = formatCurrency(lot.currentPrice);

    card.innerHTML = `
        <div class="lot-card-header">
            <div class="lot-title">${escapeHtml(lot.title)}</div>
            <span class="lot-status ${statusClass}">${statusLabel}</span>
        </div>
        
        <p class="lot-description">${escapeHtml(lot.description)}</p>
        
        <div class="lot-info">
            <span class="lot-category">📁 ${escapeHtml(lot.categoryName)}</span>
            <span class="lot-seller">👤 ${escapeHtml(lot.sellerUsername)}</span>
        </div>
        
        <div class="lot-prices">
            <div class="price-item">
                <div class="price-label">Стартова</div>
                <div class="price-value">${startingPrice}</div>
            </div>
            <div class="price-item">
                <div class="price-label">Поточна</div>
                <div class="price-value">${currentPrice}</div>
            </div>
        </div>
    `;

    return card;
}

/**
 * Показ деталей лота в модальному вікні
 */
function showLotDetails(lot) {
    const modalBody = document.getElementById('modalBody');
    const statusLabel = formatStatus(lot.status);

    modalBody.innerHTML = `
        <h2 class="modal-title">${escapeHtml(lot.title)}</h2>
        
        <div class="modal-field">
            <div class="modal-label">Статус:</div>
            <div class="modal-value">
                <span class="lot-status status-${lot.status.toLowerCase()}">${statusLabel}</span>
            </div>
        </div>

        <div class="modal-field">
            <div class="modal-label">Опис:</div>
            <div class="modal-value">${escapeHtml(lot.description)}</div>
        </div>

        <div class="modal-field">
            <div class="modal-label">Категорія:</div>
            <div class="modal-value">${escapeHtml(lot.categoryName)}</div>
        </div>

        <div class="modal-field">
            <div class="modal-label">Продавець:</div>
            <div class="modal-value">${escapeHtml(lot.sellerUsername)}</div>
        </div>

        <div class="modal-field">
            <div class="modal-label">Стартова ціна:</div>
            <div class="modal-value">${formatCurrency(lot.startingPrice)}</div>
        </div>

        <div class="modal-field">
            <div class="modal-label">Поточна ціна (макс. ставка):</div>
            <div class="modal-value">${formatCurrency(lot.currentPrice)}</div>
        </div>

        <div class="modal-field">
            <div class="modal-label">Час початку торгів:</div>
            <div class="modal-value">${new Date(lot.startTime).toLocaleString('uk-UA')}</div>
        </div>

        <div class="modal-field">
            <div class="modal-label">Час завершення торгів:</div>
            <div class="modal-value">${new Date(lot.endTime).toLocaleString('uk-UA')}</div>
        </div>

        <div class="modal-field">
            <div class="modal-label">ID лота:</div>
            <div class="modal-value">#${lot.id}</div>
        </div>
    `;

    modal.style.display = 'flex';
}

/**
 * Закриття модального вікна
 */
function closeModal() {
    modal.style.display = 'none';
}

/**
 * Форматування статусу на українську
 */
function formatStatus(status) {
    const statusMap = {
        'Pending': 'Очікує підтвердження',
        'Active': 'Активні торги',
        'Sold': 'Продано',
        'NotSold': 'Не продано',
        'Cancelled': 'Скасовано'
    };
    return statusMap[status] || status;
}

/**
 * Форматування грошей
 */
function formatCurrency(amount) {
    return new Intl.NumberFormat('uk-UA', {
        style: 'currency',
        currency: 'UAH'
    }).format(amount);
}

/**
 * Екранування HTML символів для безпеки
 */
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Завантаження всіх лотів при завантаженні сторінки
 */
document.addEventListener('DOMContentLoaded', () => {
    // Виконуємо пошук з порожними параметрами (отримаємо всі лоти)
    loadingElement.style.display = 'block';
    handleSearch(new Event('submit'));
});
