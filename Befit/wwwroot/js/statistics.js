class Statistics {
    static init() {
        this.loadStatistics();
    }

    static async loadStatistics() {
        try {
            const response = await fetch('/api/statistics');
            if (response.ok) {
                const stats = await response.json();
                this.displayStatistics(stats);
            } else {
                this.displayError('Nie udało się załadować statystyk');
            }
        } catch (error) {
            console.error('Failed to load statistics:', error);
            this.displayError('Nie udało się załadować statystyk');
        }
    }

    static displayStatistics(stats) {
        const container = document.getElementById('statisticsList');

        if (stats.length === 0) {
            container.innerHTML = `
                <div class="col-12 text-center">
                    <div class="alert alert-warning">
                        <h5>Brak danych statystycznych</h5>
                        <p class="mb-0">Nie masz jeszcze żadnych treningów z ćwiczeniami z ostatnich 4 tygodni.<br>
                        Dodaj treningi i ćwiczenia, aby zobaczyć swoje statystyki.</p>
                    </div>
                </div>
            `;
            return;
        }

        container.innerHTML = '';

        stats.forEach(stat => {
            const statCard = `
                <div class="col-md-6 mb-4">
                    <div class="card h-100">
                        <div class="card-body">
                            <h5 class="card-title">${stat.exerciseType}</h5>
                            <div class="row">
                                <div class="col-6">
                                    <p class="mb-1"><strong>Wykonane razy:</strong></p>
                                    <p class="mb-1"><strong>Łączne powtórzenia:</strong></p>
                                    <p class="mb-1"><strong>Średnie obciążenie:</strong></p>
                                    <p class="mb-0"><strong>Maksymalne obciążenie:</strong></p>
                                </div>
                                <div class="col-6">
                                    <p class="mb-1">${stat.timesPerformed}</p>
                                    <p class="mb-1">${stat.totalRepetitions}</p>
                                    <p class="mb-1">${stat.averageWeight ? stat.averageWeight.toFixed(1) + ' kg' : 'Brak danych'}</p>
                                    <p class="mb-0">${stat.maxWeight ? stat.maxWeight.toFixed(1) + ' kg' : 'Brak danych'}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            container.innerHTML += statCard;
        });
    }

    static displayError(message) {
        console.error(message);
        alert('Błąd: ' + message);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    Statistics.init();
});