﻿//(EN) Call income dashboard functions | (BG) Извиква функциите на таблото за приходи
document.addEventListener('DOMContentLoaded', function () {
    setupChart();
    setupPaginationLinks();
    setupLiveSearch('incomeSearch', 'dailyIncomeTable');
    setupYearDropdown();
    animateIncomeCards();
});
//(EN) Setup daily income chart | (BG) Настройва графиката на дневния приход
let incomeChartInstance = null;
function setupChart() {
    if (typeof incomeLabels === 'undefined' || typeof incomeDataPoints === 'undefined' || incomeLabels.length === 0) {
        console.log("No income data to show.");
        return;
    }

    const ctx = document.getElementById('incomeChart')?.getContext('2d');
    if (!ctx) return;

    if (incomeChartInstance) {
        incomeChartInstance.destroy();
    }

    incomeChartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: incomeLabels,
            datasets: [{
                label: 'Daily Income',
                data: incomeDataPoints,
                fill: true,
                tension: 0.4,
                borderWidth: 2,
                pointRadius: 4,
                pointHoverRadius: 6,
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                }
            },
            scales: {
                x: {
                    ticks: { autoSkip: true, maxTicksLimit: 10 }
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return '$' + value.toLocaleString();
                        }
                    }
                }
            }
        }
    });
}
//(EN) Setup pagination for income table | (BG) Настройва страницирането на таблицата с приходи
function setupPaginationLinks() {
    document.querySelectorAll('.pagination-link').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const url = this.getAttribute('href');
            fetch(url)
                .then(response => response.text())
                .then(html => {
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, 'text/html');
                    const newTable = doc.querySelector('#dailyIncomeTable');
                    document.querySelector('#dailyIncomeTable').innerHTML = newTable.innerHTML;

                    setupPaginationLinks();
                    setupLiveSearch('incomeSearch', 'dailyIncomeTable');
                });
        });
    });
}
//(EN) Setup live search for income | (BG) Настройва живото-търсене на приходи
function setupLiveSearch(inputId, tableId) {
    const searchInput = document.getElementById(inputId);
    if (!searchInput) return;

    searchInput.addEventListener('input', function () {
        const searchText = this.value;

        fetch(`/Admin/SearchDailyIncome?query=${encodeURIComponent(searchText)}`)
            .then(response => response.json())
            .then(data => {
                const tableBody = document.querySelector(`#${tableId} tbody`);
                tableBody.innerHTML = '';

                if (data.length === 0) {
                    const noRow = document.createElement('tr');
                    noRow.innerHTML = `<td colspan="2" style="text-align: center;">No results found</td>`;
                    tableBody.appendChild(noRow);
                } else {
                    data.forEach(item => {
                        const row = document.createElement('tr');

                        row.setAttribute('style', 'cursor:pointer;');
                        row.setAttribute('onclick', `location.href='/Admin/OrdersOnDay?date=${encodeURIComponent(item.dateFormatted)}'`);

                        row.innerHTML = `
                            <td>${item.dateFormatted}</td>
                            <td>${item.totalIncomeFormatted}</td>
                        `;
                        tableBody.appendChild(row);
                    });
                }

                const pagination = document.querySelector(`#${tableId} .pagination`);
                if (pagination) {
                    pagination.style.display = searchText.length > 0 ? 'none' : 'flex';
                }
            });
    });
}

//(EN) Setup year filter for income | (BG) Настройва филтъра по година за приходи
function setupYearDropdown() {
    const yearDropdown = document.getElementById('yearDropdown');
    if (!yearDropdown) return;

    yearDropdown.addEventListener('change', function () {
        const selectedYear = this.value;
        fetch(`/Admin/AdminIncome?year=${selectedYear}`)
            .then(response => response.text())
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newMonthlyTable = doc.querySelector('#monthlyIncomeTable');
                document.querySelector('#monthlyIncomeTable').innerHTML = newMonthlyTable.innerHTML;

                setupYearDropdown();
            });
    });
}
//(EN) Animate income summary cards | (BG) Анимира обобщените карти за приходи
function animateIncomeCards() {
    animateIncomeCard('income-total', parseFloat(document.getElementById('income-total').getAttribute('data-value')));
    animateIncomeCard('income-this-month', parseFloat(document.getElementById('income-this-month').getAttribute('data-value')));
    animateIncomeCard('income-today', parseFloat(document.getElementById('income-today').getAttribute('data-value')));
    animateIncomeCard('income-highest-day', parseFloat(document.getElementById('income-highest-day').getAttribute('data-value')));
}
//(EN) Animate single income card | (BG) Анимира отделна карта за приход
function animateIncomeCard(id, endValue, duration = 2000) {
    const el = document.getElementById(id);
    if (!el) return;
    const increment = endValue / (duration / 16);
    let current = 0;

    function update() {
        current += increment;
        if (current >= endValue) {
            el.innerText = "$" + endValue.toFixed(2).toLocaleString();
        } else {
            el.innerText = "$" + current.toFixed(2).toLocaleString();
            requestAnimationFrame(update);
        }
    }
    update();
}
