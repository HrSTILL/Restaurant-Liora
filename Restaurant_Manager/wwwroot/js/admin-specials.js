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
                location.reload();
            } else {
                alert("Failed to delete the special offer.");
            }
        });
    }
});

function confirmDelete(id) {
    document.getElementById("deleteMenuItemId").value = id;
    document.getElementById("deleteMenuItemModal").classList.add("show");
}

function closeDeleteModal() {
    document.getElementById("deleteMenuItemModal").classList.remove("show");
}


function filterSpecials() {
    const searchValue = document.getElementById("searchSpecialsInput").value.toLowerCase();
    const rows = document.querySelectorAll("tbody tr");

    rows.forEach(row => {
        const name = row.dataset.name;
        const matchesSearch = name.includes(searchValue);

        row.style.display = matchesSearch ? "" : "none";
    });
}
