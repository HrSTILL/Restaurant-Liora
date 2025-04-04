window.openDishModal = function (id) {
    const item = window.menuData.find(m => m.id === id);

    document.body.style.overflow = "hidden"; 

    document.getElementById("dishModalName").innerText = item.name;
    document.getElementById("dishModalImage").src = item.imageUrl;
    document.getElementById("dishModalDescription").innerText = item.description || "No description available.";
    document.getElementById("dishModalPrice").innerText = `${item.price.toFixed(2)} BGN`;
    document.getElementById("dishModalCalories").innerText = item.calories ? `${item.calories} kcal` : "N/A";
    document.getElementById("dishModalAllergens").innerText = item.allergens || "None";
    document.getElementById("dishModalTags").innerText = item.tags || "";
    document.getElementById("dishModalGlutenFree").innerText = item.isGlutenFree ? "Yes ✅" : "No ❌";
    document.getElementById("dishModalPrepTime").innerText = `${item.prepTimeMinutes} minutes` || "N/A";

    const modal = document.getElementById("dishModal");
    modal.style.display = "flex";
    modal.classList.add("show");
    setTimeout(() => modal.classList.add("visible"), 10);
};


    window.closeDishModal = function () {
        const modal = document.getElementById("dishModal");
        if (!modal) return;

        document.body.style.overflow = "auto"; 

        modal.classList.remove("visible");
        setTimeout(() => {
            modal.style.display = "none";
            modal.classList.remove("show");
        }, 200);
    };

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            window.closeDishModal();
        }
    });
