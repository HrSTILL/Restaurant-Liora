document.addEventListener("DOMContentLoaded", function () {
    loadSummaryCards();
    loadRevenueChart();
    loadReservationChart();
});

function animateCount(id, value, duration = 2000) {
    const el = document.getElementById(id);
    if (!el) return;

    const end = parseFloat(value);
    const increment = end / (duration / 16);
    let current = 0;

    const isCurrency = id === "card-total-revenue";

    function update() {
        current += increment;
        if (current >= end) {
            el.innerText = isCurrency
                ? `$${end.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                : Math.round(end).toLocaleString();
        } else {
            el.innerText = isCurrency
                ? `$${current.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                : Math.round(current).toLocaleString();
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
                    maintainAspectRatio: false,
                    animation: {
                        duration: 1500,
                        easing: 'easeOutBounce'
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    let label = context.dataset.label || '';
                                    if (label) {
                                        label += ': ';
                                    }
                                    if (context.parsed.y !== null) {
                                        label += `$${context.parsed.y.toFixed(2)}`;
                                    }
                                    return label;
                                }
                            }
                        },
                        legend: {
                            display: true,
                            position: 'top'
                        }
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
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top'
                        }
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
