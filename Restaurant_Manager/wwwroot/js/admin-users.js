let selectedUserId = null;

document.addEventListener("DOMContentLoaded", () => {
    const rows = document.querySelectorAll("#userTable tbody tr");

    rows.forEach(row => {
        row.addEventListener("click", () => {
            rows.forEach(r => r.classList.remove("selected"));
            row.classList.add("selected");
            selectedUserId = row.getAttribute("data-id");

            document.getElementById("editBtn").disabled = false;
            document.getElementById("deleteBtn").disabled = false;
            const detailsBtn = document.getElementById("detailsBtn");
            if (detailsBtn) detailsBtn.disabled = false;
        });
    });

    const createForm = document.getElementById("createUserForm");
    if (createForm) {
        createForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            const formData = new FormData(this);
            const action = window.location.pathname.includes("ManageUsers") ? "/Admin/CreateUser" : "/Admin/CreateStaff";

            const response = await fetch(action, {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                closeModal('createUserModal');
                setTimeout(() => location.reload(), 500);
            } else {
                alert("Error creating user.");
            }
        });
    }

    const editForm = document.getElementById("editUserForm");
    if (editForm) {
        editForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            const formData = new FormData(this);

            const response = await fetch("/Admin/EditUser", {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                closeModal('editUserModal');
                setTimeout(() => location.reload(), 500);
            } else {
                alert("Error updating user.");
            }
        });
    }

    const deleteForm = document.getElementById("deleteUserForm");
    if (deleteForm) {
        deleteForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            const userId = document.getElementById("deleteUserId").value;

            const response = await fetch("/Admin/DeleteUser", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(userId)
            });

            if (response.ok) {
                closeModal('deleteUserModal');
                setTimeout(() => location.reload(), 500);
            } else {
                alert("Error deleting user.");
            }
        });
    }

});

function openCreateModal() {
    selectedUserId = null;
    document.getElementById("createUserForm").reset();
    openModal('createUserModal');
}

function openEditModal() {
    if (!selectedUserId) return;

    fetch(`/Admin/GetUserById?id=${selectedUserId}`)
        .then(res => res.json())
        .then(data => {
            document.getElementById("editUserId").value = data.id;
            document.getElementById("editUsername").value = data.username;
            document.getElementById("editFirstName").value = data.firstName;
            document.getElementById("editLastName").value = data.lastName;
            document.getElementById("editEmail").value = data.email;
            document.getElementById("editPhone").value = data.phone;
            openModal('editUserModal');
        });
}

function confirmDelete() {
    if (!selectedUserId) return;
    document.getElementById("deleteUserId").value = selectedUserId;
    openModal('deleteUserModal');
}

function openModal(id) {
    document.getElementById(id).classList.add('show');
}

function closeModal(id) {
    document.getElementById(id).classList.remove('show');
}

function viewDetails() {
    if (!selectedUserId) return;
    document.getElementById("detailsModal").classList.add("show");
}

function goToOrders() {
    if (!selectedUserId) return;
    window.location.href = `/Admin/UserOrders/${selectedUserId}`;
}

function goToReservations() {
    if (!selectedUserId) return;
    window.location.href = `/Admin/UserReservations/${selectedUserId}`;
}


function closeDetailsModal() {
    document.getElementById("detailsModal").classList.remove("show");
}

document.addEventListener("click", function (e) {
    const clickedInside = e.target.closest("tr[data-id], .action-buttons button");
    if (!clickedInside) {
        document.querySelectorAll("#userTable tbody tr").forEach(r => r.classList.remove("selected"));
        selectedUserId = null;

        document.getElementById("editBtn").disabled = true;
        document.getElementById("deleteBtn").disabled = true;
        const detailsBtn = document.getElementById("detailsBtn");
        if (detailsBtn) detailsBtn.disabled = true;
    }
});
