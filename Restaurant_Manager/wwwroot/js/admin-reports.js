function getFormattedDate(date) {
    return date.toISOString().split('T')[0];
}

let revenueChartInstance = null;

function loadRevenueData(mode = "last7") {
    const url = `/Reports/GetRevenueData?mode=${mode}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            const labels = data.map(x => x.date);
            const revenues = data.map(x => x.totalRevenue);

            const ctx = document.getElementById('revenueChart').getContext('2d');

            if (revenueChartInstance) {
                revenueChartInstance.destroy();
            }

            revenueChartInstance = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Revenue (лв)',
                        data: revenues,
                        backgroundColor: 'rgba(54, 162, 235, 0.6)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        })
        .catch(error => {
            console.error("Error loading revenue data:", error);
        });
}

function onRevenueModeChange() {
    const mode = document.getElementById("revenueMode").value;
    loadRevenueData(mode);
}

let reservationChartInstance = null;

function loadReservationData(mode = "last7") {
    const url = `/Reports/GetReservationData?mode=${mode}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            const labels = data.map(x => x.date);
            const counts = data.map(x => x.count);

            const ctx = document.getElementById('reservationChart').getContext('2d');

            if (reservationChartInstance) {
                reservationChartInstance.destroy();
            }

            reservationChartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Number of Reservations',
                        data: counts,
                        backgroundColor: 'rgba(255, 206, 86, 0.3)',  
                        borderColor: 'rgba(255, 206, 86, 1)',        
                        borderWidth: 2,
                        tension: 0.3,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                precision: 0
                            }
                        }
                    }
                }
            });
        })
        .catch(error => {
            console.error("Error loading reservation data:", error);
        });
}

let popularItemsChartInstance = null;

function loadPopularItemsData() {
    const url = `/Reports/GetPopularItemsData`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            const labels = data.map(x => x.itemName);
            const quantities = data.map(x => x.quantity);

            const ctx = document.getElementById('popularItemsChart').getContext('2d');

            if (popularItemsChartInstance) {
                popularItemsChartInstance.destroy();
            }

            popularItemsChartInstance = new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Top Menu Items',
                        data: quantities,
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.6)',
                            'rgba(255, 159, 64, 0.6)',
                            'rgba(255, 205, 86, 0.6)',
                            'rgba(75, 192, 192, 0.6)',
                            'rgba(54, 162, 235, 0.6)',
                            'rgba(153, 102, 255, 0.6)',
                            'rgba(201, 203, 207, 0.6)'
                        ],
                        borderColor: [
                            'rgba(255, 99, 132, 1)',
                            'rgba(255, 159, 64, 1)',
                            'rgba(255, 205, 86, 1)',
                            'rgba(75, 192, 192, 1)',
                            'rgba(54, 162, 235, 1)',
                            'rgba(153, 102, 255, 1)',
                            'rgba(201, 203, 207, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: {
                        duration: 1000,
                        easing: 'easeOutQuart'
                    },
                    plugins: {
                        datalabels: {
                            color: '#333',
                            font: {
                                weight: 'bold'
                            },
                            formatter: (value, context) => {
                                const total = context.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                                const percent = (value / total * 100).toFixed(1);
                                return `${percent}%`;
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    const value = context.parsed;
                                    const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                    const percent = ((value / total) * 100).toFixed(1);
                                    return [
                                        `Quantity: ${value}`,
                                        `Percentage: ${percent}%`
                                    ];
                                }
                            }
                        },
                        legend: {
                            position: 'top'
                        }
                    }
                },
                plugins: [ChartDataLabels]
            });

        })
        .catch(error => {
            console.error("Error loading popular items data:", error);
        });
}

function loadSummaryCards() {
    fetch('/Reports/GetSummaryCardsData')
        .then(response => response.json())
        .then(data => {
            animateCount("card-total-revenue", data.totalRevenue, " лв");
            animateCount("card-total-orders", data.totalOrders);
            animateCount("card-total-reservations", data.totalReservations);
            document.getElementById("card-top-day").innerText = data.topDay || "—";
        })
        .catch(error => {
            console.error("Error loading summary cards data:", error);
        });
}

function animateCount(id, value, suffix = '', duration = 1000) {
    const el = document.getElementById(id);
    const start = 0;
    const end = parseFloat(value);
    const increment = end / (duration / 16);
    let current = start;

    function update() {
        current += increment;
        if (current >= end) {
            el.innerText = Math.round(end) + suffix;
        } else {
            el.innerText = Math.round(current) + suffix;
            requestAnimationFrame(update);
        }
    }

    update();
}

function onReservationModeChange() {
    const mode = document.getElementById("reservationMode").value;
    loadReservationData(mode);
}

document.addEventListener("DOMContentLoaded", function () {
    loadRevenueData("last7");
    loadReservationData("last7");
    loadPopularItemsData();
    loadSummaryCards();
});
