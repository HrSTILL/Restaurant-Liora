﻿// (EN) Setup delete and search | (BG) Настройка на изтриване и търсене
document.addEventListener("DOMContentLoaded", () => {
    const deleteForm = document.getElementById("deleteMenuItemForm");

    if (deleteForm) {
        deleteForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            const id = document.getElementById("deleteMenuItemId").value;

            const response = await fetch(`/Admin/DeleteMenuItemAjax/${id}`, {
                method: "POST"
            });

            if (response.ok) {
                closeDeleteModal();
                createToast('success', 'fa-solid fa-circle-check', 'Success', 'Menu item deleted successfully!');
                setTimeout(() => location.reload(), 1500); 
            } else {
                createToast('error', 'fa-solid fa-circle-exclamation', 'Error', 'Failed to delete the menu item.');
            }
        });
    }

    setupLiveSpecialSearch();
});
// (EN) Open delete confirmation modal | (BG) Отваря модал за потвърждение на изтриване
function confirmDelete(id) {
    document.getElementById("deleteMenuItemId").value = id;
    document.getElementById("deleteMenuItemModal").classList.add("show");
}
// (EN) Close delete confirmation modal | (BG) Затваря модал за потвърждение на изтриване
function closeDeleteModal() {
    document.getElementById("deleteMenuItemModal").classList.remove("show");
}
// (EN) Setup live search for specials | (BG) Настройва търсене на специални оферти
function setupLiveSpecialSearch() {
    const searchInput = document.getElementById("searchSpecialsInput");
    if (!searchInput) return;

    searchInput.addEventListener("input", function () {
        const searchText = this.value.toLowerCase();
        const rows = document.querySelectorAll("#specialOffersTableBody tr");

        rows.forEach(row => {
            const name = row.dataset.name || "";
            const matchesSearch = name.includes(searchText);

            if (matchesSearch) {
                row.style.display = ""; 
            } else {
                row.style.display = "none"; 
            }
        });
    });
}
