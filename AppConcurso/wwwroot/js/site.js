let graficoDashboard = null;

window.renderizarGraficoDashboard = function (dados) {
    const ctx = document.getElementById('graficoDashboard').getContext('2d');

    const labels = Object.keys(dados);
    const valores = Object.values(dados);

    // Destroi o gráfico anterior se existir
    if (graficoDashboard !== null) {
        graficoDashboard.destroy();
    }

    graficoDashboard = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Empréstimos',
                data: valores,
                borderColor: '#0d6efd',
                backgroundColor: 'rgba(13, 110, 253, 0.2)',
                tension: 0.3,
                fill: true
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
