// (EN) Load mini dashboard charts | (BG) Зарежда мини графики на таблото
document.addEventListener("DOMContentLoaded", function () {
    new Chart(document.getElementById('salesMiniChart'), {
        type: 'bar',
        data: {
            labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
            datasets: [{
                data: [120, 150, 170, 90, 200],
                backgroundColor: '#4CAF50'
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: { x: { display: false }, y: { display: false } },
            responsive: true,
            maintainAspectRatio: false
        }
    });
    new Chart(document.getElementById('occupancyMiniChart'), {
        type: 'line',
        data: {
            labels: ['10am', '12pm', '2pm', '4pm', '6pm'],
            datasets: [{
                data: [2, 5, 4, 6, 8],
                borderColor: '#FF9800',
                tension: 0.3,
                fill: true,
                backgroundColor: 'rgba(255, 152, 0, 0.2)'
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: { x: { display: false }, y: { display: false } },
            responsive: true,
            maintainAspectRatio: false
        }
    });
    new Chart(document.getElementById('preferencesMiniChart'), {
        type: 'pie',
        data: {
            labels: ['Salad', 'Steak', 'Pasta'],
            datasets: [{
                data: [40, 30, 30],
                backgroundColor: ['#03A9F4', '#E91E63', '#FFC107']
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            responsive: true,
            maintainAspectRatio: false
        }
    });
});
