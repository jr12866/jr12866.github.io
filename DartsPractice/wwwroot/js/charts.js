// Ensure global namespace handles safe initialization checks
window.renderDartsDashboardCharts = function (gameLabels, gameCounts, timelineLabels, turnAverages) {
    // 1. Verify canvas DOM nodes are fully attached before invoking Chart engines
    const canvas1 = document.getElementById('gameDistributionChart');
    const canvas2 = document.getElementById('turnProgressionChart');

    if (!canvas1 || !canvas2) {
        console.warn("Chart drawing aborted: Canvas DOM elements are not ready yet.");
        return;
    }

    // 2. Clean up active contexts to protect memory bounds
    const existingChart1 = Chart.getChart("gameDistributionChart");
    const existingChart2 = Chart.getChart("turnProgressionChart");
    if (existingChart1) existingChart1.destroy();
    if (existingChart2) existingChart2.destroy();

    // 3. Render Game Distribution Chart
    new Chart(canvas1, {
        type: 'bar',
        data: {
            labels: gameLabels,
            datasets: [{
                data: gameCounts,
                backgroundColor: ['#2a9d8f', '#e63946', '#f4a261', '#457b9d'],
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            animation: { duration: 0 },
            scales: {
                y: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { display: false } },
                x: { grid: { display: false } }
            }
        }
    });

    // 4. Render Turn Efficiency Progress Line Chart
    new Chart(canvas2, {
        type: 'line',
        data: {
            labels: timelineLabels,
            datasets: [{
                label: 'Avg Turn Counts',
                data: turnAverages,
                borderColor: '#457b9d',
                backgroundColor: 'rgba(69, 123, 157, 0.08)',
                fill: true,
                tension: 0.2,
                pointRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: { duration: 0 },
            scales: {
                y: { beginAtZero: false, grid: { display: false } },
                x: { grid: { display: false } }
            }
        }
    });
};
