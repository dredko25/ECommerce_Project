document.addEventListener('DOMContentLoaded', () => {

    updateNavigation();

    if (document.getElementById('catalog-container')) {
        loadProducts();
    }

    if (document.getElementById('product-details-container')) {
        loadProductDetails();
    }

    if (document.getElementById('cart-container')) {
        loadCart();
    }
});

async function loadProducts() {
    const container = document.getElementById('catalog-container');

    try {
        const response = await fetch('/api/products');

        if (!response.ok) {
            throw new Error(`Помилка HTTP: ${response.status}`);
        }

        const data = await response.json();

        container.innerHTML = '';

        if (!data.items || data.items.length === 0) {
            container.innerHTML = '<p class="text-center text-muted">Каталог поки що порожній.</p>';
            return;
        }

        data.items.forEach(product => {
            const imageUrl = product.imageUrl || 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?q=80&w=600&auto=format&fit=crop';

            const cardHtml = `
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
            container.innerHTML += cardHtml;
        });

    } catch (error) {
        console.error('Помилка при завантаженні:', error);
    }
}

window.addToCart = async function (productId) {
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');

    console.log("addToCart: productId =", productId);
    console.log("userId з localStorage =", userId);

    if (!token || !userId || userId === 'undefined' || userId === 'null') {
        alert('Будь ласка, увійдіть в систему, щоб додати товар у кошик.');
        window.location.href = '/Login';
        return;
    }

    try {
        const url = `/api/cart/user/${userId}/items`;

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                productId: productId,
                quantity: 1
            })
        });

        console.log("Статус відповіді:", response.status);

        if (!response.ok) {
            const errorText = await response.text();
            console.error("Помилка від сервера:", errorText);

            if (response.status === 401) {
                localStorage.removeItem('token');
                localStorage.removeItem('userId');
                alert('Сесія закінчилася. Увійдіть знову.');
                window.location.href = '/Login';
                return;
            }

            throw new Error(`Не вдалося додати товар (статус ${response.status})`);
        }

        const updatedCart = await response.json();
        console.log("Кошик успішно оновлено:", updatedCart);

        alert('Товар додано в кошик!');

        if (document.getElementById('cart-container')) {
            loadCart();
        }

    } catch (error) {
        console.error("Помилка в addToCart:", error);
        alert(error.message || 'Не вдалося додати товар у кошик. Спробуйте ще раз.');
    }
};

async function loadProductDetails() {
    const container = document.getElementById('product-details-container');
    const spinner = document.getElementById('loading-spinner');

    const urlParams = new URLSearchParams(window.location.search);
    const productId = urlParams.get('id');

    if (!productId) {
        container.innerHTML = '<div class="alert alert-danger">Товар не знайдено.</div>';
        return;
    }

    try {
        const response = await fetch(`/api/products/${productId}`);
        if (!response.ok) throw new Error('Товар не знайдено');

        const product = await response.json();
        const imageUrl = product.imageUrl || 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?q=80&w=600';

        if (spinner) spinner.remove();

        container.innerHTML = `
            <div class="row">
                <div class="col-md-6 mb-4">
                    <img src="${imageUrl}" class="img-fluid rounded shadow-sm" alt="${product.name}">
                </div>
                <div class="col-md-6 d-flex flex-column justify-content-center">
                    <p class="text-uppercase text-muted tracking-wide">${product.categoryName || 'Категорія'}</p>
                    <h2 class="display-5 mb-3">${product.name}</h2>
                    <h3 class="mb-4">${product.price} грн</h3>
                    <p class="lead text-muted mb-5">${product.description || 'Елегантна прикраса, яка підкреслить ваш стиль.'}</p>
                    
                    <button onclick="addToCart('${product.id}')" class="btn btn-dark btn-lg w-100 rounded-0 py-3">
                        ДОДАТИ В КОШИК
                    </button>
                </div>
            </div>
        `;
    } catch (error) {
        if (spinner) spinner.remove();
        container.innerHTML = '<div class="alert alert-danger">Помилка завантаження товару.</div>';
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

        console.log("Відповідь:", data.user);

        const exactToken = data.accessToken;

        let exactUserId = null;
        if (data.user && data.user.id) exactUserId = data.user.id;

        if (!exactUserId) {
            alert("Бекенд не повернув ID! Відкрийте консоль (F12), щоб побачити структуру.");
            return;
        }

        localStorage.setItem('token', exactToken);
        localStorage.setItem('userId', exactUserId);

        //alert('Ви успішно увійшли!');
        window.location.href = '/';
    } catch (error) {
        alert(error.message);
    }
}

function updateNavigation() {
    const token = localStorage.getItem('token');

    const navLogin = document.getElementById('nav-login');
    const navRegister = document.getElementById('nav-register');
    const navCart = document.getElementById('nav-cart');
    const navLogout = document.getElementById('nav-logout');

    if (token) {
        if (navLogin) navLogin.classList.add('d-none');
        if (navRegister) navRegister.classList.add('d-none');
        if (navCart) navCart.classList.remove('d-none');
        if (navLogout) navLogout.classList.remove('d-none');
    } else {
        if (navLogin) navLogin.classList.remove('d-none');
        if (navRegister) navRegister.classList.remove('d-none');
        if (navCart) navCart.classList.add('d-none');
        if (navLogout) navLogout.classList.add('d-none');
    }
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

            if (textResponse) {
                try {
                    const errorData = JSON.parse(textResponse);
                    if (errorData.errors) {
                        const validationMessages = Object.values(errorData.errors).flat().join('\n');
                        throw new Error(`Помилка заповнення даних:\n${validationMessages}`);
                    }
                } catch (jsonError) {
                    throw new Error(`Помилка сервера (${response.status}).`);
                }
            }

            throw new Error(`Сервер відхилив запит (Статус ${response.status}) без пояснень.`);
        }

        alert('Реєстрація успішна! Тепер ви можете увійти.');
        window.location.href = '/Login';
    } catch (error) {
        alert(error.message);
    }
}


window.handleLogout = function () {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');

    updateNavigation();
    window.location.href = '/';
}

async function loadCart() {
    const container = document.getElementById('cart-container');
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');

    if (!token || !userId) {
        window.location.href = '/Login';
        return;
    }

    try {

        const response = await fetch(`/api/cart/user/${userId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

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
                    <td class="text-center">${item.quantity} шт.</td>
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

        const response = await fetch(`/api/cart/items/${cartItemId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        console.log("Статус видалення:", response.status);

        if (response.ok) {
            console.log("Товар успішно видалено");
            loadCart();
        }
        else if (response.status === 401) {
            alert('Сесія закінчилася. Увійдіть знову.');
            localStorage.clear();
            window.location.href = '/Login';
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