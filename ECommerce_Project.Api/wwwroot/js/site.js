if (typeof API_URL === 'undefined') {
    window.API_URL = '/api/products';
}

document.addEventListener('DOMContentLoaded', () => {
    updateNavigation();

    if (document.getElementById('catalog-container')) {
        loadFilterCategories();
        loadCatalogProducts();

        document.getElementById('searchInput')?.addEventListener('keyup', (e) => {
            if (e.key === 'Enter') applyFilters();
        });

        document.getElementById('categoryFilter')?.addEventListener('change', applyFilters);
    }

    if (document.getElementById('product-details-container')) {
        loadProductDetails();
    }

    if (document.getElementById('cart-container')) {
        loadCart();
    }
});



async function loadFilterCategories() {
    const filterSelect = document.getElementById('categoryFilter');
    if (!filterSelect) return;

    try {
        const response = await fetch('/api/categories');
        if (response.ok) {
            const data = await response.json();
            const categories = Array.isArray(data) ? data : (data.items || []);

            categories.forEach(cat => {
                filterSelect.innerHTML += `<option value="${cat.id}">${cat.name}</option>`;
            });
        }
    } catch (error) {
        console.error('Помилка завантаження категорій:', error);
    }
}

let currentSearch = '';
let currentCategory = '';

window.applyFilters = function () {
    currentSearch = document.getElementById('searchInput').value;
    currentCategory = document.getElementById('categoryFilter').value;

    loadCatalogProducts(currentSearch, currentCategory, 1);
}

function getUserRole() {
    const token = localStorage.getItem('token');
    if (!token) {
        console.warn("Токен не знайдено в localStorage");
        return null;
    }

    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payload = JSON.parse(jsonPayload);

        console.log("Повний Payload токена:", payload);

        const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            || payload['role']
            || payload['roles'];

        console.log("Знайдена роль:", role);
        return role;
    } catch (e) {
        console.error("Помилка декодування токена", e);
        return null;
    }
}

function getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

function updateNavigation() {
    const token = localStorage.getItem('token');
    const role = getUserRole();

    const navLogin = document.getElementById('nav-login');
    const navRegister = document.getElementById('nav-register');
    const navCart = document.getElementById('nav-cart');
    const navLogout = document.getElementById('nav-logout');
    const navAdmin = document.getElementById('nav-admin');

    if (token) {
        if (navLogin) navLogin.classList.add('d-none');
        if (navRegister) navRegister.classList.add('d-none');
        if (navCart) navCart.classList.remove('d-none');
        if (navLogout) navLogout.classList.remove('d-none');

        if (navAdmin) {
            if (role === 'Admin') {
                navAdmin.classList.remove('d-none');
            } else {
                navAdmin.classList.add('d-none');
            }
        }
    } else {
        if (navLogin) navLogin.classList.remove('d-none');
        if (navRegister) navRegister.classList.remove('d-none');
        if (navCart) navCart.classList.add('d-none');
        if (navLogout) navLogout.classList.add('d-none');
        if (navAdmin) navAdmin.classList.add('d-none');
    }
}

async function handleLogin(event) {
    event.preventDefault();

    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const response = await fetch('/api/users/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) throw new Error('Неправильний логін або пароль');

        const data = await response.json();
        const exactToken = data.accessToken;
        const exactRefreshToken = data.refreshToken;

        let exactUserId = null;
        if (data.user && data.user.id) exactUserId = data.user.id;

        if (!exactUserId) {
            alert("Бекенд не повернув ID!");
            return;
        }

        localStorage.setItem('token', exactToken);
        localStorage.setItem('refreshToken', exactRefreshToken);
        localStorage.setItem('userId', exactUserId);

        window.location.href = '/';
    } catch (error) {
        alert(error.message);
    }
}

async function attemptRefreshToken() {
    const userId = localStorage.getItem('userId');
    const refreshToken = localStorage.getItem('refreshToken');

    if (!userId || !refreshToken) return false;

    try {
        const response = await fetchWithAuth('/api/users/refresh-token', {
            method: 'POST',
            body: JSON.stringify({ userId: userId, refreshToken: refreshToken })
        });

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('token', data.accessToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            return true;
        }
    } catch (error) {
        console.error("Помилка при спробі оновити токен:", error);
    }

    return false;
}

async function fetchWithAuth(url, options = {}) {
    options.headers = {
        ...options.headers,
        ...getAuthHeaders()
    };

    let response = await fetch(url, options);

    if (response.status === 401) {
        console.warn("Токен протермінований.");

        const isRefreshed = await attemptRefreshToken();

        if (isRefreshed) {
            console.log("Оновлення успішне. Повторюємо оригінальний запит.");
            options.headers['Authorization'] = `Bearer ${localStorage.getItem('token')}`;
            response = await fetch(url, options);
        } else {
            console.error("Сесія закінчена.");
            window.handleLogout();
            throw new Error('Ваша сесія закінчилась. Будь ласка, увійдіть знову.');
        }
    }

    return response;
}

async function handleRegister(event) {
    event.preventDefault();

    const firstName = document.getElementById('regFirstName').value;
    const lastName = document.getElementById('regLastName').value;
    const email = document.getElementById('regEmail').value;
    const contactNumber = document.getElementById('regPhone').value;
    const password = document.getElementById('regPassword').value;

    try {
        const response = await fetch('/api/users/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ firstName, lastName, contactNumber, email, password })
        });

        if (!response.ok) {
            const textResponse = await response.text();
            throw new Error('Помилка реєстрації: ' + textResponse);
        }

        alert('Реєстрація успішна! Тепер ви можете увійти.');
        window.location.href = '/Login';
    } catch (error) {
        alert(error.message);
    }
}

window.handleLogout = async function () {
    const token = localStorage.getItem('token');

    if (token) {
        try {
            await fetch('/api/users/logout', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            console.log("Сервер успішно знищив рефреш-токен");
        } catch (error) {
            console.error("Помилка при логауті на сервері:", error);
        }
    }

    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userId');

    updateNavigation();
    window.location.href = '/';
}

async function loadCatalogProducts(search = '', categoryId = '', page = 1) {
    const container = document.getElementById('catalog-container');
    const paginationContainer = document.getElementById('pagination-container');

    container.innerHTML = `
        <div class="text-center w-100" id="loading-spinner">
            <div class="spinner-border text-secondary" role="status">
                <span class="visually-hidden">Завантаження...</span>
            </div>
        </div>
    `;

    if (paginationContainer) paginationContainer.innerHTML = '';

    try {
        let url = `/api/products?pageSize=12&pageNumber=${page}`;

        if (search) {
            url += `&search=${encodeURIComponent(search)}`;
        }
        if (categoryId) {
            url += `&categoryId=${categoryId}`;
        }

        const response = await fetch(url);
        if (!response.ok) throw new Error(`Помилка HTTP: ${response.status}`);
        const data = await response.json();

        container.innerHTML = '';
        if (!data.items || data.items.length === 0) {
            container.innerHTML = '<p class="text-center text-muted">За вашим запитом нічого не знайдено.</p>';
            return;
        }

        data.items.forEach(product => {
            const imageUrl = product.imageUrl || 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?q=80&w=600&auto=format&fit=crop';
            container.innerHTML += `
                <div class="col-md-4 col-sm-6 mb-4">
                    <div class="card h-100 border-0 shadow-sm">
                        <img src="${imageUrl}" class="card-img-top" alt="${product.name}" style="height: 300px; object-fit: cover;">
                        <div class="card-body text-center d-flex flex-column">
                            <p class="text-muted small mb-1 text-uppercase">${product.categoryName || 'Прикраса'}</p>
                            <a href="/Product?id=${product.id}" class="text-decoration-none text-dark">
                                <h5 class="card-title mb-3">${product.name}</h5>
                            </a>
                            <div class="mt-auto">
                                <h6 class="mb-3" style="font-size: 1.2rem;">${product.price} грн</h6>
                                <button onclick="addToCart('${product.id}')" class="btn btn-outline-dark w-100 rounded-0">В кошик</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });

        if (paginationContainer && data.totalCount > 0) {
            const totalPages = Math.ceil(data.totalCount / data.pageSize);
            renderPaginationControls(totalPages, data.pageNumber, paginationContainer);
        }

    } catch (error) {
        console.error('Помилка при завантаженні:', error);
        container.innerHTML = '<p class="text-center text-danger">Сталася помилка при завантаженні товарів.</p>';
    }
}

function renderPaginationControls(totalPages, currentPage, container) {
    if (totalPages <= 1) {
        container.innerHTML = '';
        return;
    }

    let html = '<nav><ul class="pagination pagination-lg justify-content-center border-0">';

    const prevDisabled = currentPage === 1 ? 'disabled' : '';
    html += `
        <li class="page-item ${prevDisabled}">
            <button class="page-link text-dark rounded-0" 
                    onclick="loadCatalogProducts(currentSearch, currentCategory, ${currentPage - 1})">
                &laquo;
            </button>
        </li>
    `;

    for (let i = 1; i <= totalPages; i++) {
        const activeClass = i === currentPage ? 'active bg-dark border-dark text-white' : 'text-dark';

        html += `
            <li class="page-item">
                <button class="page-link rounded-0 ${activeClass}" 
                        onclick="loadCatalogProducts(currentSearch, currentCategory, ${i})">
                    ${i}
                </button>
            </li>
        `;
    }

    const nextDisabled = currentPage === totalPages ? 'disabled' : '';
    html += `
        <li class="page-item ${nextDisabled}">
            <button class="page-link text-dark rounded-0" 
                    onclick="loadCatalogProducts(currentSearch, currentCategory, ${currentPage + 1})">
                &raquo;
            </button>
        </li>
    `;

    html += '</ul></nav>';
    container.innerHTML = html;
}

async function loadProductDetails() {
    const container = document.getElementById('product-details-container');
    const urlParams = new URLSearchParams(window.location.search);
    const productId = urlParams.get('id');

    if (!productId) return;

    try {
        const response = await fetch(`/api/products/${productId}`);
        if (!response.ok) throw new Error('Товар не знайдено');

        const product = await response.json();
        const imageUrl = product.imageUrl || 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?q=80&w=600';

        container.innerHTML = `
            <div class="row">
                <div class="col-md-6 mb-4">
                    <img src="${imageUrl}" class="img-fluid rounded shadow-sm" alt="${product.name}">
                </div>
                <div class="col-md-6 d-flex flex-column justify-content-center">
                    <p class="text-uppercase text-muted tracking-wide">${product.categoryName || 'Категорія'}</p>
                    <h2 class="display-5 mb-3">${product.name}</h2>
                    <h3 class="mb-4">${product.price} грн</h3>
                    <p class="lead text-muted mb-5">${product.description || ''}</p>
                    <button onclick="addToCart('${product.id}')" class="btn btn-dark btn-lg w-100 rounded-0 py-3">ДОДАТИ В КОШИК</button>
                </div>
            </div>
        `;
    } catch (error) {
        container.innerHTML = '<div class="alert alert-danger">Помилка завантаження товару.</div>';
    }
}

window.addToCart = async function (productId) {
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');

    if (!token || !userId || userId === 'undefined' || userId === 'null') {
        alert('Будь ласка, увійдіть в систему.');
        window.location.href = '/Login';
        return;
    }

    try {
        const response = await fetchWithAuth(`/api/cart/user/${userId}/items`, {
            method: 'POST',
            body: JSON.stringify({ productId: productId, quantity: 1 })
        });

        if (!response.ok) throw new Error('Не вдалося додати товар');
        if (document.getElementById('cart-container')) loadCart();

    } catch (error) {
        alert(error.message);
    }
};

async function loadCart() {
    const container = document.getElementById('cart-container');
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');

    if (!token || !userId) {
        window.location.href = '/Login';
        return;
    }

    try {

        const response = await fetchWithAuth(`/api/cart/user/${userId}`);

        if (response.status === 404) {
            renderEmptyCart(container);
            return;
        }

        if (!response.ok) throw new Error('Помилка завантаження кошика');

        const cart = await response.json();

        if (!cart.items || cart.items.length === 0) {
            renderEmptyCart(container);
            return;
        }

        let html = `
            <div class="table-responsive">
                <table class="table align-middle">
                    <thead class="table-light">
                        <tr>
                            <th>Прикраса</th>
                            <th>Ціна</th>
                            <th class="text-center">Кількість</th>
                            <th>Сума</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
        `;

        let grandTotal = 0;

        cart.items.forEach(item => {
            const itemTotal = item.unitPrice * item.quantity;
            grandTotal += itemTotal;

            const imageUrl = item.productImageUrl || 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?q=80&w=150';

            html += `
                <tr>
                    <td>
                        <div class="d-flex align-items-center">
                            <img src="${imageUrl}" class="rounded me-3" style="width: 60px; height: 60px; object-fit: cover;">
                            <span class="fw-bold">${item.productName}</span>
                        </div>
                    </td>
                    <td>${item.unitPrice} грн</td>
                    <td>
                        <div class="input-group input-group-sm mx-auto" style="width: 120px;">
                            <button class="btn btn-outline-secondary" type="button"
                                    onclick="changeQuantity('${item.id}', -1, ${item.quantity})">-</button>
        
                            <input type="number" class="form-control text-center" value="${item.quantity}" min="1" 
                                   onchange="updateQuantity('${item.id}', this.value)"
                                   onkeypress="if(event.key === 'Enter') this.blur();">
        
                            <button class="btn btn-outline-secondary" type="button" 
                                    onclick="changeQuantity('${item.id}', 1, ${item.quantity})">+</button>
                        </div>
                    </td>
                    <td class="fw-bold">${itemTotal} грн</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-outline-danger" onclick="removeFromCart('${item.id}')">
                            Видалити
                        </button>
                    </td>
                </tr>
            `;
        });

        html += `
                    </tbody>
                </table>
            </div>
            <div class="d-flex justify-content-end mt-4">
                <h3 style="font-family: 'Georgia', serif;">Разом: ${grandTotal} грн</h3>
            </div>
            <div class="d-flex justify-content-end mt-3">
                <button class="btn btn-dark btn-lg rounded-0 px-5">ОФОРМИТИ ЗАМОВЛЕННЯ</button>
            </div>
        `;

        container.innerHTML = html;

    } catch (error) {
        container.innerHTML = `<div class="alert alert-danger text-center">${error.message}</div>`;
    }
}

window.changeQuantity = function (cartItemId, delta, currentQuantity) {
    const newQuantity = parseInt(currentQuantity) + delta;

    if (newQuantity < 1) return;

    updateQuantity(cartItemId, newQuantity);
}


window.updateQuantity = async function (cartItemId, newQuantity) {
    const userId = localStorage.getItem('userId');
    const quantity = parseInt(newQuantity);

  
    if (isNaN(quantity) || quantity < 1) {
        loadCart(); 
        return;
    }

    try {
        const response = await fetchWithAuth(`/api/cart/user/${userId}/items/${cartItemId}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(quantity)
        });

        if (response.ok) {
            loadCart();
        } else {
            alert('Не вдалося оновити кількість товару');
            loadCart();
        }
    } catch (error) {
        console.error("Помилка при оновленні кількості:", error);
    }
}

function renderEmptyCart(container) {
    container.innerHTML = `
        <div class="text-center py-5">
            <h4 class="text-muted mb-4">Ваш кошик наразі порожній</h4>
            <a href="/" class="btn btn-outline-dark rounded-0 px-4">Повернутися до каталогу</a>
        </div>
    `;
}

window.removeFromCart = async function (cartItemId) {
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');

    if (!token || !userId) {
        alert('Сесія закінчилася. Увійдіть знову.');
        window.location.href = '/Login';
        return;
    }

    if (!confirm('Видалити цей товар з кошика?')) {
        return;
    }

    try {
        console.log(`Видаляємо cartItemId: ${cartItemId}`);

        const response = await fetchWithAuth(`/api/cart/items/${cartItemId}`, {
            method: 'DELETE'
        });

        console.log("Статус видалення:", response.status);

        if (response.ok) {
            console.log("Товар успішно видалено");
            loadCart();
        }
        else if (response.status === 404) {
            alert('Товар вже видалено або не знайдено в кошику.');
            loadCart();
        }
        else {
            const errorText = await response.text();
            console.error("Помилка видалення:", errorText);
            alert('Не вдалося видалити товар. Спробуйте ще раз.');
        }
    } catch (error) {
        console.error("Помилка при видаленні:", error);
        alert('Помилка з\'єднання з сервером.');
    }
};