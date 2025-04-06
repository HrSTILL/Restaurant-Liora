window.openDishModal = function (id) {
    const item = window.menuData.find(m => m.id === id);

    document.body.style.top = `-${window.scrollY}px`;
    document.body.classList.add("modal-open");

    document.getElementById("dishModalName").innerText = item.name;
    document.getElementById("dishModalImage").src = item.imageUrl;
    document.getElementById("dishModalDescription").innerText = item.description || "No description available.";
    document.getElementById("dishModalPrice").innerText = `${item.price.toFixed(2)} BGN`;
    document.getElementById("dishModalCalories").innerText = item.calories ? `${item.calories} kcal` : "N/A";
    document.getElementById("dishModalAllergens").innerText = item.allergens || "None";
    document.getElementById("dishModalTags").innerText = item.tags || "";
    document.getElementById("dishModalGlutenFree").innerText = item.isGlutenFree ? "Yes ✅" : "No ❌";
    document.getElementById("dishModalPrepTime").innerText = item.prepTimeMinutes ? `${item.prepTimeMinutes} minutes` : "N/A";

    const modal = document.getElementById("dishModal");
    modal.style.display = "flex";
    requestAnimationFrame(() => {
        modal.classList.add("show", "visible");
    });
};

window.closeDishModal = function () {
    const modal = document.getElementById("dishModal");
    if (!modal) return;

    modal.classList.remove("visible");

    setTimeout(() => {
        modal.classList.remove("show");
        modal.style.display = "none";

        const scrollY = document.body.style.top;
        document.body.classList.remove("modal-open");
        document.body.style.top = "";
        window.scrollTo(0, parseInt(scrollY || "0") * -1);
    }, 200);
};

document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
        window.closeDishModal();
    }
});

function showReservationPrompt() {
    const modal = document.getElementById("reservationPromptModal");
    if (modal) {
        modal.classList.add("show");
    }
}

function closeReservationPrompt() {
    const modal = document.getElementById("reservationPromptModal");
    if (modal) {
        modal.classList.remove("show");
    }
}

window.tryAddToCart = async function (menuItemId) {
    try {
        const checkRes = await fetch('/Cart/CheckReservation');
        const checkResult = await checkRes.json();

        if (!checkResult.success) {
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
            alert("Failed to add item.");
        }
    } catch (err) {
        console.error("Error during cart operation:", err);
    }
};

window.tryAddToCartFromModal = function () {
    const name = document.getElementById("dishModalName").innerText;
    const item = window.menuData.find(m => m.name === name);
    if (item) {
        tryAddToCart(item.id);
    }
};

function updateCartCount(count) {
    const badge = document.getElementById("cartBadge");
    if (badge) {
        badge.innerText = count;
        badge.style.display = count > 0 ? "inline-block" : "none";

        badge.classList.add("shake");
        setTimeout(() => badge.classList.remove("shake"), 400);
    }
}
