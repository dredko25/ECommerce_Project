
document.addEventListener('DOMContentLoaded', () => {

    const container = document.getElementById('catalog-container');
    if (container) {
        loadProducts();
    } else {
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

        // Очищаємо контейнер від спінера
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
                            <h5 class="card-title mb-3">${product.name}</h5>
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

window.addToCart = function (productId) {
    alert(`Товар ${productId} додано!`);
}