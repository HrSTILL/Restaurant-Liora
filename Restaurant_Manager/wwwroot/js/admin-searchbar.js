document.addEventListener('DOMContentLoaded', function () {
    const ordersInput = document.getElementById('searchOrdersInput');
    if (ordersInput) {
        ordersInput.addEventListener('input', filterOrders);
    }

    const reservationsInput = document.getElementById('reservationSearchInput');
    if (reservationsInput) {
        reservationsInput.addEventListener('input', filterReservations);
    }
});

function filterReservations() {
    filterTable("reservationSearchInput", "reservationTable", "data-user");
}

function filterOrders() {
    filterTable("searchOrdersInput", "ordersTable", "data-name");
}

function filterTable(inputId, tableId, dataAttr) {
    const searchInput = document.getElementById(inputId);
    const rows = document.querySelectorAll(`#${tableId} tbody tr`);

    const search = searchInput.value.toLowerCase();
    rows.forEach(row => {
        const data = row.getAttribute(dataAttr) || '';
        row.style.display = data.includes(search) ? '' : 'none';
    });
}
