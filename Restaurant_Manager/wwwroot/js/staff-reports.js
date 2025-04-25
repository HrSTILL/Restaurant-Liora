function animateCount(id, value, duration = 2000) {
    const el = document.getElementById(id);
    const start = 0;
    const end = parseFloat(value);
    const range = end - start;
    const increment = range / (duration / 16);
    let current = start;

    function update() {
        current += increment;
        if (current >= end) {
            el.innerText = Math.round(end);
        } else {
            el.innerText = Math.round(current);
            requestAnimationFrame(update);
        }
    }

    update();
}

function loadSummaryCards() {
    fetch('/Staff/GetStaffSummary')
        .then(response => response.json())
        .then(data => {
            animateCount("card-total-revenue", data.totalRevenue);
            animateCount("card-total-orders", data.totalOrders);
            animateCount("card-total-reservations", data.totalReservations);
        });
}

function loadRevenueChart() {
    fetch('/Staff/GetStaffRevenueChart')
        .then(response => response.json())
        .then(data => {
            const ctx = document.getElementById('revenueChart').getContext('2d');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: data.map(x => x.date),
                    datasets: [{
                        label: 'Revenue (лв)',
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
                    maintainAspectRatio: false,
                    animation: {
                        duration: 1500,
                        easing: 'easeOutBounce'
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        });
}

function loadReservationChart() {
    fetch('/Staff/GetStaffReservationChart')
        .then(response => response.json())
        .then(data => {
            const ctx = document.getElementById('reservationChart').getContext('2d');
            new Chart(ctx, {
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
                    maintainAspectRatio: false,
                    animation: {
                        duration: 1500,
                        easing: 'easeOutQuart'
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        });
}

document.addEventListener("DOMContentLoaded", function () {
    loadSummaryCards();
    loadRevenueChart();
    loadReservationChart();
});
