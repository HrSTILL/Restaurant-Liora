document.addEventListener("DOMContentLoaded", () => {
    const deleteForm = document.getElementById("deleteMenuItemForm");
    const searchInput = document.getElementById("searchInput");
    const categoryFilter = document.getElementById("categoryFilter");

    if (deleteForm) {
        deleteForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            const id = document.getElementById("deleteMenuItemId").value;

            const response = await fetch(`/Admin/DeleteMenuItemAjax/${id}`, {
                method: "POST"
            });

            if (response.ok) {
                closeDeleteModal();
                location.reload();
            } else {
                alert("Failed to delete the item.");
            }
        });
    }

    if (searchInput) {
        searchInput.addEventListener("input", filterMenu);
    }

    if (categoryFilter) {
        categoryFilter.addEventListener("change", filterMenu);
    }
});

function confirmDelete(id) {
    document.getElementById("deleteMenuItemId").value = id;
    document.getElementById("deleteMenuItemModal").classList.add("show");
}

function closeDeleteModal() {
    document.getElementById("deleteMenuItemModal").classList.remove("show");
}

function filterMenu() {
    const searchValue = document.getElementById("searchInput").value.toLowerCase();
    const selectedCategory = document.getElementById("categoryFilter").value;
    const rows = document.querySelectorAll("#menuTableBody tr");

    rows.forEach(row => {
        const name = row.dataset.name;
        const category = row.dataset.category;
        const matchesSearch = name.includes(searchValue);
        const matchesCategory = selectedCategory === "all" || category === selectedCategory;

        row.style.display = (matchesSearch && matchesCategory) ? "" : "none";
    });
}
