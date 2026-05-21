const API_BASE_URL = '/api';
const TOKEN_STORAGE_KEY = 'auction.jwt';
let currentUserRole = 'Unregistered';
let currentUserProfile = null;
let categoriesCache = [];

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

document.getElementById('createLotForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const token = getToken();

    const payload = {
        title: document.getElementById('newLotTitle').value,
        description: document.getElementById('newLotDesc').value,
        startingPrice: parseFloat(document.getElementById('newLotPrice').value),
        categoryId: parseInt(document.getElementById('newLotCategory').value, 10),
        startTime: new Date(document.getElementById('newLotStart').value).toISOString(),
        endTime: new Date(document.getElementById('newLotEnd').value).toISOString()
    };

    if (currentUserRole === 'Admin' || currentUserRole === 'Manager') {
        payload.status = parseInt(document.getElementById('newLotStatus').value, 10);
    }

    try {
        const res = await fetch(`${API_BASE_URL}/lots`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            throw new Error(await res.text());
        }

        alert('Лот успішно створено!');
        handleSearch(new Event('submit'));
    } catch (error) {
        alert(`Помилка створення: ${error.message}`);
    }
});

document.getElementById('createCategoryForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const token = getToken();

    const payload = {
        name: document.getElementById('newCategoryName').value
    };

    try {
        const res = await fetch(`${API_BASE_URL}/categories`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            throw new Error(await res.text());
        }

        alert('Категорію створено!');
        document.getElementById('newCategoryName').value = '';
        loadCategories();
    } catch (error) {
        alert(`Помилка: ${error.message}`);
    }
});

initializeAuthState();

async function handleSearch(e) {
    e.preventDefault();

    const searchQuery = document.getElementById('searchQuery').value || null;
    const minPrice = document.getElementById('minPrice').value || null;
    const maxPrice = document.getElementById('maxPrice').value || null;
    const status = document.getElementById('status').value || null;
    const categoryId = document.getElementById('categoryId').value || null;

    noResultsElement.style.display = 'none';
    errorElement.style.display = 'none';
    loadingElement.style.display = 'block';
    lotsList.innerHTML = '';

    try {
        const params = new URLSearchParams();
        if (searchQuery) params.append('searchQuery', searchQuery);
        if (minPrice) params.append('minPrice', minPrice);
        if (maxPrice) params.append('maxPrice', maxPrice);
        if (status) params.append('status', status);
        if (categoryId) params.append('categoryId', categoryId);

        const response = await fetch(`${API_BASE_URL}/lots/search?${params}`);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const lots = await response.json();
        loadingElement.style.display = 'none';

        if (lots.length === 0) {
            noResultsElement.style.display = 'block';
            return;
        }

        displayLots(lots);
    } catch (error) {
        loadingElement.style.display = 'none';
        errorElement.textContent = `❌ Помилка: ${error.message}`;
        errorElement.style.display = 'block';
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
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await response.json();

        if (!response.ok) throw new Error(data?.error || 'Помилка авторизації');

        const token = data.token || data.Token;
        if (!token) throw new Error('Сервер не повернув JWT token');

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
        currentUserProfile = null;
        currentUserOutput.innerHTML = 'Користувач не авторизований';
        updateAuthBadge(false);
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/auth/me`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        const data = await response.json();

        if (!response.ok) throw new Error(data?.error || 'Не вдалося отримати профіль');

        currentUserProfile = data;
        currentUserOutput.innerHTML = `
            <div style="background: #1e293b; padding: 15px; border-radius: 8px;">
                <p style="margin-bottom: 8px;"><strong>👤 Ім'я:</strong> ${escapeHtml(data.username)}</p>
                <p style="margin-bottom: 8px;"><strong>📧 Email:</strong> ${escapeHtml(data.email)}</p>
                <p style="margin-bottom: 0;"><strong>🔑 Роль:</strong> ${escapeHtml(data.role)}</p>
            </div>
        `;
        updateAuthBadge(true, data);
    } catch (error) {
        currentUserProfile = null;
        currentUserOutput.innerHTML = `❌ ${error.message}`;
        updateAuthBadge(false);
    }
}

function initializeAuthState() {
    const token = getToken();
    if (token) {
        tokenOutput.value = token;
        loadCurrentUser();
    } else {
        currentUserProfile = null;
        tokenOutput.value = '';
        currentUserOutput.innerHTML = 'Користувач не авторизований';
        updateAuthBadge(false);
    }
}

function logout() {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    currentUserProfile = null;
    tokenOutput.value = '';
    currentUserOutput.innerHTML = 'Користувач не авторизований';
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

function initDateInputs() {
    const startInput = document.getElementById('newLotStart');
    const endInput = document.getElementById('newLotEnd');

    if (startInput && endInput) {
        const now = new Date();
        now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
        const nowStr = now.toISOString().slice(0, 16);

        startInput.min = nowStr;
        endInput.min = nowStr;
        startInput.value = nowStr;

        const endDate = new Date(now);
        endDate.setDate(endDate.getDate() + 7);
        endInput.value = endDate.toISOString().slice(0, 16);
    }
}

function updateAuthBadge(isAuthenticated, username = '') {
    const displayName = typeof username === 'string'
        ? username
        : (username?.username || username?.Username || '');

    authStatusBadge.className = `status-pill ${isAuthenticated ? 'status-authenticated' : 'status-guest'}`;
    authStatusBadge.textContent = isAuthenticated ? `Auth${displayName ? `: ${displayName}` : ''}` : 'Guest';

    const createLotSection = document.getElementById('createLotSection');
    const adminStatusGroup = document.getElementById('adminStatusGroup');
    const createCatSection = document.getElementById('createCategorySection');

    if (isAuthenticated && typeof username === 'object' && username) {
        currentUserRole = username.role || username.Role || 'Unregistered';
        if (createLotSection) createLotSection.style.display = 'block';

        if (currentUserRole === 'Admin' || currentUserRole === 'Manager') {
            if (adminStatusGroup) adminStatusGroup.style.display = 'block';
            if (createCatSection) createCatSection.style.display = 'block';
        } else {
            if (adminStatusGroup) adminStatusGroup.style.display = 'none';
            if (createCatSection) createCatSection.style.display = 'none';
        }
        initDateInputs();
    } else {
        currentUserRole = 'Unregistered';
        if (createLotSection) createLotSection.style.display = 'none';
        if (adminStatusGroup) adminStatusGroup.style.display = 'none';
        if (createCatSection) createCatSection.style.display = 'none';
    }
}

async function loadCategories() {
    try {
        const res = await fetch(`${API_BASE_URL}/categories`);
        const categories = await res.json();
        categoriesCache = Array.isArray(categories) ? categories : [];

        const searchSelect = document.getElementById('categoryId');
        const createSelect = document.getElementById('newLotCategory');

        const searchOptionsHtml = buildCategoryOptions(null, '-- Всі категорії --');
        const formOptionsHtml = buildCategoryOptions(null, '-- Оберіть категорію --');

        if (searchSelect) searchSelect.innerHTML = searchOptionsHtml;
        if (createSelect) createSelect.innerHTML = formOptionsHtml;
    } catch (e) {
        console.error('Не вдалося завантажити категорії', e);
    }
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

function displayLots(lots) {
    lotsList.innerHTML = '';
    lots.forEach(lot => {
        const lotCard = createLotCard(lot);
        lotsList.appendChild(lotCard);
    });
}

function createLotCard(lot) {
    const card = document.createElement('div');
    card.className = 'lot-card';
    card.onclick = () => showLotDetails(lot);

    const statusClass = `status-${lot.status.toLowerCase()}`;
    const statusLabel = formatStatus(lot.status);
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

    if (currentUserRole === 'Admin' || currentUserRole === 'Manager') {
        const lotJson = encodeURIComponent(JSON.stringify(lot));
        card.innerHTML += `
            <div style="display: flex; gap: 10px; margin-top: 10px;">
                <button onclick="deleteLot(event, ${lot.id})" class="btn btn-danger" style="flex: 1;">🗑 Видалити</button>
                <button onclick="showEditForm(event, '${lotJson}')" class="btn btn-secondary" style="flex: 1;">✏️ Редагувати</button>
            </div>
        `;
    }

    return card;
}

async function deleteLot(e, lotId) {
    e.stopPropagation();
    if (!confirm('Ви впевнені, що хочете видалити цей лот?')) return;

    try {
        await fetch(`${API_BASE_URL}/lots/${lotId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });
        handleSearch(new Event('submit'));
    } catch (err) {
        alert('Помилка видалення.');
    }
}

function showEditForm(e, lotJsonStr) {
    e.stopPropagation();
    const lot = JSON.parse(decodeURIComponent(lotJsonStr));
    const modalBody = document.getElementById('modalBody');
    const startValue = toDateTimeLocalValue(lot.startTime);
    const endValue = toDateTimeLocalValue(lot.endTime);

    const statusMap = { 'Pending': 0, 'Active': 1, 'Cancelled': 2, 'Sold': 3, 'NotSold': 4 };
    const currentStatusVal = statusMap[lot.status] ?? 0;

    modalBody.innerHTML = `
        <h2 class="modal-title">Редагувати лот #${lot.id}</h2>
        <form id="editLotForm" class="search-form">
            <div class="form-group">
                <label>Назва:</label>
                <input type="text" id="editLotTitle" class="input-field" value="${escapeHtml(lot.title)}" required>
            </div>
            <div class="form-group">
                <label>Опис:</label>
                <textarea id="editLotDesc" class="input-field" required>${escapeHtml(lot.description)}</textarea>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label>Стартова ціна:</label>
                    <input type="number" id="editLotPrice" class="input-field" value="${lot.startingPrice}" required min="1">
                </div>
                <div class="form-group">
                    <label>Категорія:</label>
                    <select id="editLotCategory" class="input-field" required>
                        ${buildCategoryOptions(lot.categoryId, '-- Оберіть категорію --')}
                    </select>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label>Час початку:</label>
                    <input type="datetime-local" id="editLotStart" class="input-field" value="${startValue}" required>
                </div>
                <div class="form-group">
                    <label>Час завершення:</label>
                    <input type="datetime-local" id="editLotEnd" class="input-field" value="${endValue}" required>
                </div>
            </div>
            <div class="form-group">
                <label>Статус:</label>
                <select id="editLotStatus" class="input-field">
                    <option value="0" ${currentStatusVal === 0 ? 'selected' : ''}>Pending</option>
                    <option value="1" ${currentStatusVal === 1 ? 'selected' : ''}>Active</option>
                    <option value="2" ${currentStatusVal === 2 ? 'selected' : ''}>Cancelled</option>
                    <option value="3" ${currentStatusVal === 3 ? 'selected' : ''}>Sold</option>
                    <option value="4" ${currentStatusVal === 4 ? 'selected' : ''}>NotSold</option>
                </select>
            </div>
            <button type="submit" class="btn btn-primary" style="margin-top: 15px;">Зберегти</button>
        </form>
    `;

    modal.style.display = 'flex';

    document.getElementById('editLotForm').addEventListener('submit', async (ev) => {
        ev.preventDefault();
        const payload = {
            title: document.getElementById('editLotTitle').value,
            description: document.getElementById('editLotDesc').value,
            startingPrice: parseFloat(document.getElementById('editLotPrice').value),
            status: parseInt(document.getElementById('editLotStatus').value, 10),
            categoryId: parseInt(document.getElementById('editLotCategory').value, 10),
            startTime: new Date(document.getElementById('editLotStart').value).toISOString(),
            endTime: new Date(document.getElementById('editLotEnd').value).toISOString()
        };

        try {
            const res = await fetch(`${API_BASE_URL}/lots/${lot.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${getToken()}`
                },
                body: JSON.stringify(payload)
            });

            if (!res.ok) throw new Error("Помилка при оновленні");

            closeModal();
            handleSearch(new Event('submit'));
        } catch (err) {
            alert(err.message);
        }
    });
}

function showLotDetails(lot) {
    const modalBody = document.getElementById('modalBody');
    const statusLabel = formatStatus(lot.status);
    const actionHtml = renderLotActions(lot);

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
        ${actionHtml}
    `;

    modal.style.display = 'flex';
    attachLotModalActions(lot);
}

function closeModal() {
    modal.style.display = 'none';
}

function renderLotActions(lot) {
    const canBid = canPlaceBid(lot);
    const canApprove = canApproveLot(lot);

    if (!canBid && !canApprove) {
        return currentUserProfile
            ? `<div class="lot-actions-note">Додаткових дій для цього лота немає.</div>`
            : `<div class="lot-actions-note">Увійдіть, щоб робити ставки або керувати лотами.</div>`;
    }

    const minBid = Number(lot.currentPrice || lot.startingPrice || 0) + 1;

    return `
        <div class="lot-actions">
            ${canBid ? `
                <form id="bidForm" class="lot-action-card">
                    <h3>Зробити ставку</h3>
                    <p class="lot-action-note">Мінімальна ставка: ${formatCurrency(minBid)}</p>
                    <label class="form-group">
                        <span class="form-label-inline">Сума ставки</span>
                        <input type="number" id="bidAmount" class="input-field" min="${minBid}" step="0.01" value="${minBid}" required>
                    </label>
                    <button type="submit" class="btn btn-primary btn-full">Підтвердити ставку</button>
                </form>
            ` : ''}
            ${canApprove ? `
                <div class="lot-action-card">
                    <h3>Керування лотом</h3>
                    <p class="lot-action-note">Лот очікує підтвердження.</p>
                    <button type="button" id="approveLotButton" class="btn btn-secondary btn-full">Підтвердити лот</button>
                </div>
            ` : ''}
        </div>
    `;
}

function attachLotModalActions(lot) {
    const bidForm = document.getElementById('bidForm');
    if (bidForm) {
        bidForm.addEventListener('submit', async (event) => {
            event.preventDefault();

            const amount = parseFloat(document.getElementById('bidAmount').value);
            try {
                const response = await fetch(`${API_BASE_URL}/auction/bids`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${getToken()}`
                    },
                    body: JSON.stringify({ lotId: lot.id, amount })
                });

                const data = await response.json();
                if (!response.ok) {
                    throw new Error(data?.message || data?.error || 'Не вдалося прийняти ставку');
                }

                alert(data?.message || 'Ставку успішно прийнято!');
                closeModal();
                handleSearch(new Event('submit'));
            } catch (error) {
                alert(`Помилка ставки: ${error.message}`);
            }
        });
    }

    const approveButton = document.getElementById('approveLotButton');
    if (approveButton) {
        approveButton.addEventListener('click', async () => {
            if (!currentUserProfile?.id) {
                alert('Не вдалося визначити поточного користувача.');
                return;
            }

            try {
                const response = await fetch(`${API_BASE_URL}/lots/${lot.id}/approve?managerId=${currentUserProfile.id}`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${getToken()}`
                    }
                });

                if (!response.ok) {
                    throw new Error(await response.text());
                }

                alert('Лот підтверджено');
                closeModal();
                handleSearch(new Event('submit'));
            } catch (error) {
                alert(`Помилка підтвердження: ${error.message}`);
            }
        });
    }
}

function canPlaceBid(lot) {
    return Boolean(
        currentUserProfile &&
        (currentUserRole === 'Registered' || currentUserRole === 'Admin') &&
        lot.status === 'Active' &&
        currentUserProfile.username !== lot.sellerUsername
    );
}

function canApproveLot(lot) {
    return Boolean(
        currentUserProfile &&
        (currentUserRole === 'Admin' || currentUserRole === 'Manager') &&
        lot.status === 'Pending'
    );
}

function buildCategoryOptions(selectedCategoryId = null, placeholder = '-- Всі категорії --') {
    let optionsHtml = `<option value="">${placeholder}</option>`;

    categoriesCache.forEach(category => {
        const isSelected = selectedCategoryId !== null && Number(selectedCategoryId) === Number(category.id);
        optionsHtml += `<option value="${category.id}" ${isSelected ? 'selected' : ''}>${escapeHtml(category.name)}</option>`;
    });

    return optionsHtml;
}

function toDateTimeLocalValue(value) {
    const date = new Date(value);
    const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return localDate.toISOString().slice(0, 16);
}

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

function formatCurrency(amount) {
    return new Intl.NumberFormat('uk-UA', {
        style: 'currency',
        currency: 'UAH'
    }).format(amount);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

document.addEventListener('DOMContentLoaded', () => {
    loadingElement.style.display = 'block';
    loadCategories();
    handleSearch(new Event('submit'));
});