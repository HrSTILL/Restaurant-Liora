function filterTable(inputId, tableId, attribute = "data-user") {
    const inputElement = document.getElementById(inputId);
    if (!inputElement) return;

    const input = inputElement.value.toLowerCase();
    const rows = document.querySelectorAll(`#${tableId} tbody tr`);

    rows.forEach(row => {
        const userData = row.getAttribute(attribute)?.toLowerCase() || "";
        if (userData.includes(input)) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
}

function filterReservations() {
    filterTable("reservationSearchInput", "reservationTable", "data-user");
}

function filterOrders() {
    filterTable("searchOrdersInput", "ordersTable", "data-name");
}
