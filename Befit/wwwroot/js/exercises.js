class Exercises {
    static init() {
        this.checkUserRole();
        this.loadExercises();
    }

    static async checkUserRole() {
        try {
            const response = await fetch('/api/auth/is-admin');
            const data = await response.json();

            if (data.isAdmin) {
                document.getElementById('addExerciseBtn').classList.remove('d-none');
                document.getElementById('adminInfo').classList.remove('d-none');
                this.setupEventListeners();
            } else {
                document.getElementById('userInfo').classList.remove('d-none');
            }
        } catch (error) {
            console.error('Role check error:', error);
            document.getElementById('userInfo').classList.remove('d-none');
        }
    }

    static setupEventListeners() {
        document.getElementById('addExerciseBtn').addEventListener('click', () => {
            this.openModal();
        });

        document.getElementById('saveExerciseBtn').addEventListener('click', this.saveExercise.bind(this));
    }

    static async loadExercises() {
        try {
            const response = await fetch('/api/exercise-types');
            if (response.ok) {
                const exercises = await response.json();
                this.displayExercises(exercises);
            } else {
                this.displayError('Nie udało się załadować ćwiczeń');
            }
        } catch (error) {
            console.error('Failed to load exercises:', error);
            this.displayError('Nie udało się załadować ćwiczeń');
        }
    }

    static displayExercises(exercises) {
        const container = document.getElementById('exercisesList');

        if (exercises.length === 0) {
            container.innerHTML = `
                <div class="col-12 text-center">
                    <p class="text-muted">Brak dostępnych ćwiczeń.</p>
                </div>
            `;
            return;
        }

        container.innerHTML = '';

        exercises.forEach(exercise => {
            const isAdmin = !document.getElementById('adminInfo').classList.contains('d-none');

            const buttonsHtml = isAdmin ? `
                <div class="mt-3">
                    <button class="btn btn-sm btn-outline-primary" onclick="Exercises.editExercise(${exercise.id})">Edytuj</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="Exercises.deleteExercise(${exercise.id})">Usuń</button>
                </div>
            ` : '';

            const exerciseCard = `
                <div class="col-md-4 mb-4">
                    <div class="card h-100">
                        <div class="card-body">
                            <h5 class="card-title">${exercise.name}</h5>
                            ${exercise.description ? `<p class="card-text">${exercise.description}</p>` : ''}
                            ${exercise.muscleGroup ? `<span class="badge bg-primary">${exercise.muscleGroup}</span>` : ''}
                            ${buttonsHtml}
                        </div>
                    </div>
                </div>
            `;
            container.innerHTML += exerciseCard;
        });
    }

    static openModal(exercise = null) {
        const modal = new bootstrap.Modal(document.getElementById('exerciseModal'));
        const modalTitle = document.getElementById('modalTitle');
        const exerciseId = document.getElementById('exerciseId');
        const exerciseName = document.getElementById('exerciseName');
        const exerciseDescription = document.getElementById('exerciseDescription');
        const exerciseMuscleGroup = document.getElementById('exerciseMuscleGroup');

        if (exercise) {
            modalTitle.textContent = 'Edytuj Ćwiczenie';
            exerciseId.value = exercise.id;
            exerciseName.value = exercise.name;
            exerciseDescription.value = exercise.description || '';
            exerciseMuscleGroup.value = exercise.muscleGroup || '';
        } else {
            modalTitle.textContent = 'Dodaj Nowe Ćwiczenie';
            exerciseId.value = '';
            exerciseName.value = '';
            exerciseDescription.value = '';
            exerciseMuscleGroup.value = '';
        }

        modal.show();
    }

    static async editExercise(id) {
        try {
            const response = await fetch(`/api/exercise-types/${id}`);
            if (response.ok) {
                const exercise = await response.json();
                this.openModal(exercise);
            } else {
                this.displayError('Nie udało się załadować ćwiczenia do edycji');
            }
        } catch (error) {
            console.error('Failed to load exercise:', error);
            this.displayError('Nie udało się załadować ćwiczenia do edycji');
        }
    }

    static async saveExercise() {
        const id = document.getElementById('exerciseId').value;
        const name = document.getElementById('exerciseName').value;
        const description = document.getElementById('exerciseDescription').value;
        const muscleGroup = document.getElementById('exerciseMuscleGroup').value;

        if (!name) {
            alert('Proszę wprowadzić nazwę ćwiczenia');
            return;
        }

        const exercise = {
            name: name,
            description: description,
            muscleGroup: muscleGroup
        };

        try {
            let response;
            if (id) {
                response = await fetch(`/api/exercise-types/${id}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(exercise)
                });
            } else {
                response = await fetch('/api/exercise-types', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(exercise)
                });
            }

            if (response.ok) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('exerciseModal'));
                modal.hide();
                this.loadExercises();
                this.showSuccess(`Ćwiczenie ${id ? 'zaktualizowane' : 'utworzone'} pomyślnie!`);
            } else {
                this.displayError(`Nie udało się ${id ? 'zaktualizować' : 'utworzyć'} ćwiczenia`);
            }
        } catch (error) {
            console.error('Failed to save exercise:', error);
            this.displayError(`Nie udało się ${id ? 'zaktualizować' : 'utworzyć'} ćwiczenia`);
        }
    }

    static async deleteExercise(id) {
        if (!confirm('Czy na pewno chcesz usunąć to ćwiczenie?')) {
            return;
        }

        try {
            const response = await fetch(`/api/exercise-types/${id}`, {
                method: 'DELETE'
            });

            if (response.ok) {
                this.loadExercises();
                this.showSuccess('Ćwiczenie usunięte pomyślnie!');
            } else {
                this.displayError('Nie udało się usunąć ćwiczenia');
            }
        } catch (error) {
            console.error('Failed to delete exercise:', error);
            this.displayError('Nie udało się usunąć ćwiczenia');
        }
    }

    static displayError(message) {
        console.error(message);
        alert('Błąd: ' + message);
    }

    static showSuccess(message) {
        console.log(message);
        alert('Sukces: ' + message);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    Exercises.init();
});