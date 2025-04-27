//(EN) Call dashboard loading functions | (BG) Извиква функциите при зареждане на таблото
document.addEventListener("DOMContentLoaded", () => {
    loadSummaryCards();
    loadRevenueChart();
    loadReservationChart();
    loadTopItemsChart();
    loadRecentOrders();
    loadUpcomingReservations();
    loadRecentActivities();
});
//(EN) Animate revenue loading | (BG) Анимира зареждането на оборота
function animateCount(id, value, isCurrency = false, duration = 2000) {
    const el = document.getElementById(id);
    const end = parseFloat(value);
    const increment = end / (duration / 16);
    let current = 0;

    function update() {
        current += increment;
        if (current >= end) {
            el.innerText = isCurrency ? `$${end.toFixed(2)}` : Math.round(end);
        } else {
            el.innerText = isCurrency ? `$${current.toFixed(2)}` : Math.round(current);
            requestAnimationFrame(update);
        }
    }
    update();
}

//(EN) Load dashboard summary cards | (BG) Зареждане на картите с обобщение на таблото
function loadSummaryCards() {
    fetch('/Admin/GetDashboardSummary')
        .then(res => res.json())
        .then(data => {
            animateCount("card-total-revenue", data.totalRevenue, true); 
            animateCount("card-total-orders", data.totalOrders);
            animateCount("card-total-staff", data.totalStaff);
            animateCount("card-total-specials", data.totalSpecials);
            animateCount("card-total-users", data.totalUsers);
            animateCount("card-total-reservations", data.totalReservations);
        });
}

//(EN) Load revenue chart  | (BG) Зареждане на графиката с оборота
function loadRevenueChart() {
    fetch('/Admin/GetDashboardRevenueChart')
        .then(res => res.json())
        .then(data => {
            new Chart(document.getElementById('revenueChart').getContext('2d'), {
                type: 'line',
                data: {
                    labels: data.map(x => x.date),
                    datasets: [{
                        label: 'Revenue ($)',
                        data: data.map(x => x.totalRevenue),
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 2,
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    animation: { duration: 1500, easing: 'easeOutBounce' },
                    scales: { y: { beginAtZero: true } }
                }
            });
        });
}
//(EN) Load reservation chart | (BG) Зареждане на графиката с резервациите
function loadReservationChart() {
    fetch('/Admin/GetDashboardReservationChart')
        .then(res => res.json())
        .then(data => {
            new Chart(document.getElementById('reservationChart').getContext('2d'), {
                type: 'bar',
                data: {
                    labels: data.map(x => x.date),
                    datasets: [{
                        label: 'Reservations',
                        data: data.map(x => x.count),
                        backgroundColor: 'rgba(255, 206, 86, 0.6)',
                        borderColor: 'rgba(255, 206, 86, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    animation: { duration: 1500, easing: 'easeOutQuart' },
                    scales: { y: { beginAtZero: true } }
                }
            });
        });
}
//(EN) Load top menu items chart | (BG) Зареждане на графиката с най-продаваните ястия
function loadTopItemsChart() {
    fetch('/Admin/GetTopMenuItems')
        .then(res => res.json())
        .then(data => {
            new Chart(document.getElementById('topItemsChart').getContext('2d'), {
                type: 'pie',
                data: {
                    labels: data.map(x => x.itemName),
                    datasets: [{
                        data: data.map(x => x.quantity),
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.6)',
                            'rgba(255, 159, 64, 0.6)',
                            'rgba(255, 205, 86, 0.6)',
                            'rgba(75, 192, 192, 0.6)',
                            'rgba(153, 102, 255, 0.6)'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    animation: { duration: 1500 },
                    plugins: { legend: { position: 'top' } }
                }
            });
        });
}
//(EN) Load recent customer orders | (BG) Зареждане на последните поръчки на клиентите
function loadRecentOrders() {
    fetch('/Admin/GetRecentOrders')
        .then(res => res.json())
        .then(data => {
            document.getElementById('recent-orders-list').innerHTML =
                `<ul>${data.map(o => `<li>${o.customerName} - $${o.total.toFixed(2)}</li>`).join('')}</ul>`;
        });
}
//(EN) Load upcoming reservation list | (BG) Зарежда списъка с предстоящи резервации
function loadUpcomingReservations() {
    fetch('/Admin/GetUpcomingReservations')
        .then(res => res.json())
        .then(data => {
            document.getElementById('upcoming-reservations-list').innerHTML =
                `<ul>${data.map(r => `<li>${r.time} - ${r.people} people - ${r.customerName}</li>`).join('')}</ul>`;
        });
}
//(EN) Load reacent activity | (BG) Зарежда последната активност
function loadRecentActivities() {
    fetch('/Admin/GetRecentActivities')
        .then(res => res.json())
        .then(data => {
            document.getElementById('activity-list').innerHTML =
                data.map(item => `<li>${item}</li>`).join('');
        });
}