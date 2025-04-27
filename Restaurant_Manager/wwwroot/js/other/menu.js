// (EN) Open dish details modal | (BG) Отваря модал с детайли за ястие
let scrollY = 0;
window.openDishModal = function (id) {
    const item = window.menuData.find(m => m.id === id);
    if (!item) return;
    scrollY = window.scrollY;
    document.body.style.position = 'fixed';
    document.body.style.top = `-${scrollY}px`;
    document.body.style.left = '0';
    document.body.style.right = '0';
    document.body.style.overflow = 'hidden';
    document.body.style.width = '100%';
    document.getElementById("dishModalName").innerText = item.name;
    document.getElementById("dishModalImage").src = item.imageUrl;
    document.getElementById("dishModalDescription").innerText = item.description || "No description available.";
    document.getElementById("dishModalPrice").innerText = `$${item.price.toFixed(2)}`;
    document.getElementById("dishModalCalories").innerText = item.calories ? `${item.calories} kcal` : "N/A";
    document.getElementById("dishModalAllergens").innerText = item.allergens || "None";
    document.getElementById("dishModalTags").innerText = item.tags || "";
    document.getElementById("dishModalGlutenFree").innerText = item.isGlutenFree ? "Yes ✅" : "No ❌";
    document.getElementById("dishModalPrepTime").innerText = `${item.prepTimeMinutes} minutes` || "N/A";
    const modal = document.getElementById("dishModal");
    modal.style.display = "flex";
    requestAnimationFrame(() => {
        modal.classList.add("show", "visible");
    });
};
// (EN) Close dish details modal | (BG) Затваря модал с детайли за ястие
window.closeDishModal = function () {
    const modal = document.getElementById("dishModal");
    if (!modal) return;
    modal.classList.remove("visible");
    setTimeout(() => {
        modal.classList.remove("show");
        modal.style.display = "none";
        document.body.style.position = '';
        document.body.style.top = '';
        document.body.style.left = '';
        document.body.style.right = '';
        document.body.style.overflow = '';
        document.body.style.width = '';
        window.scrollTo({ top: scrollY, behavior: "instant" });
    }, 200);
};
// (EN) Close dish details modal on Escape key | (BG) Затваря модал с детайли за ястие при натискане на Escape
document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
        window.closeDishModal();
    }
});
// (EN) Show reservation prompt modal | (BG) Показва модал за резервация
function showReservationPrompt() {
    const modal = document.getElementById("reservationPromptModal");
    if (modal) {
        modal.classList.add("show");
    }
}
// (EN) Close reservation prompt modal | (BG) Затваря модал за резервация
function closeReservationPrompt() {
    const modal = document.getElementById("reservationPromptModal");
    if (modal) {
        modal.classList.remove("show");
    }
}
// (EN) Try adding item to cart | (BG) Опитва да добави артикул в количката
window.tryAddToCart = async function (menuItemId) {
    try {
        const checkRes = await fetch('/Cart/CheckReservation');
        const checkResult = await checkRes.json();
        if (!checkResult.success) {
            updateCartCount(0);
            showReservationPrompt();
            return;
        }
        const res = await fetch('/Cart/AddToCartAjax', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ menuItemId })
        });
        const result = await res.json();
        if (result.success) {
            updateCartCount(result.itemCount);
        } else {
            if (result.message === "No valid reservation.") {
                updateCartCount(0);
                showReservationPrompt();
            } else {
                alert("Failed to add item: " + (result.message || "Unknown error"));
            }
        }
    } catch (err) {
        console.error("Add to cart error:", err);
        alert("Something went wrong while adding to cart.");
    }
};
// (EN) Try adding item to cart from modal | (BG) Опитва да добави артикул в количката от модал
window.tryAddToCartFromModal = function () {
    const name = document.getElementById("dishModalName").innerText;
    const item = window.menuData.find(m => m.name === name);
    if (item) {
        tryAddToCart(item.id);
    }
};
// (EN) Update cart item quantity | (BG) Актуализира количеството в количката
function updateCartCount(count) {
    const badge = document.getElementById("cartBadge");
    if (badge) {
        badge.innerText = count;
        badge.style.display = count > 0 ? "inline-block" : "none";
        badge.classList.add("shake");
        setTimeout(() => badge.classList.remove("shake"), 400);
    }
}
// // (EN) Open dish modal on card click | (BG) Отваря модала при клик върху карта
document.querySelectorAll(".menu-card").forEach(card => {
    card.addEventListener("click", function () {
        const id = this.getAttribute("data-id");
        if (id) {
            openDishModal(parseInt(id));
        }
    });
});

