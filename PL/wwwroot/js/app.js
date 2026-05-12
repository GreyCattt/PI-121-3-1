/**
 * Internet Auction UI
 * JavaScript логіка для пошуку й фільтрації лотів
 */

// API базовий URL
const API_BASE_URL = '/api';

// DOM елементи
const searchForm = document.getElementById('searchForm');
const lotsList = document.getElementById('lotsList');
const loadingElement = document.getElementById('loading');
const errorElement = document.getElementById('error');
const noResultsElement = document.getElementById('noResults');
const modal = document.getElementById('lotModal');
const closeBtn = document.querySelector('.close');

// Слухачі подій
searchForm.addEventListener('submit', handleSearch);
closeBtn.addEventListener('click', closeModal);
window.addEventListener('click', (e) => {
    if (e.target === modal) closeModal();
});

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
