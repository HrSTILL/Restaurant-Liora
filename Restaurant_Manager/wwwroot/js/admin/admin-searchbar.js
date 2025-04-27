// (EN) Setup search filters | (BG) Настройва търсачките
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
// (EN) Filter reservations | (BG) Филтрира резервации
function filterReservations() {
    filterTable("reservationSearchInput", "reservationTable", "data-user");
}
// (EN) Filter orders | (BG) Филтрира поръчки
function filterOrders() {
    filterTable("searchOrdersInput", "ordersTable", "data-name");
}
// (EN) Filter table rows | (BG) Филтрира редове в таблица
function filterTable(inputId, tableId, dataAttr) {
    const searchInput = document.getElementById(inputId);
    const rows = document.querySelectorAll(`#${tableId} tbody tr`);
    const search = searchInput.value.toLowerCase();
    rows.forEach(row => {
        const data = row.getAttribute(dataAttr) || '';
        row.style.display = data.includes(search) ? '' : 'none';
    });
}
