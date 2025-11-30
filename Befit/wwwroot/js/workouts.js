class Workouts {
    static init() {
        this.loadWorkouts();
        this.setupEventListeners();
        this.loadExerciseTypes();
    }

    static setupEventListeners() {
        document.getElementById('addWorkoutBtn').addEventListener('click', () => {
            this.openModal();
        });

        document.getElementById('saveWorkoutBtn').addEventListener('click', this.saveWorkout.bind(this));
        document.getElementById('saveExerciseBtn').addEventListener('click', this.saveExercise.bind(this));
    }

    static async loadExerciseTypes() {
        try {
            const response = await fetch('/api/exercise-types');
            if (response.ok) {
                this.exerciseTypes = await response.json();
            }
        } catch (error) {
            console.error('Failed to load exercise types:', error);
        }
    }

    static async loadWorkouts() {
        try {
            const response = await fetch('/api/workouts');
            if (response.ok) {
                const workouts = await response.json();
                await this.loadPerformedExercises(workouts);
            } else {
                this.displayError('Nie udało się załadować treningów');
            }
        } catch (error) {
            console.error('Failed to load workouts:', error);
            this.displayError('Nie udało się załadować treningów');
        }
    }

    static async loadPerformedExercises(workouts) {
        try {
            const response = await fetch('/api/performed-exercises');
            if (response.ok) {
                const performedExercises = await response.json();
                this.displayWorkouts(workouts, performedExercises);
            } else {
                this.displayWorkouts(workouts, []);
            }
        } catch (error) {
            console.error('Failed to load exercises:', error);
            this.displayWorkouts(workouts, []);
        }
    }

    static displayWorkouts(workouts, performedExercises) {
        const container = document.getElementById('workoutsList');

        if (workouts.length === 0) {
            container.innerHTML = `
                <div class="col-12 text-center">
                    <p class="text-muted">Brak treningów. Dodaj pierwszy trening!</p>
                </div>
            `;
            return;
        }

        container.innerHTML = '';

        workouts.forEach(workout => {
            const startDate = new Date(workout.startDate).toLocaleString('pl-PL');
            const endDate = new Date(workout.endDate).toLocaleString('pl-PL');
            const duration = Math.round((new Date(workout.endDate) - new Date(workout.startDate)) / (1000 * 60));
            const workoutExercises = performedExercises.filter(pe => pe.workoutId === workout.id);

            let exercisesHtml = '';
            if (workoutExercises.length > 0) {
                exercisesHtml = `
                    <div class="mt-3">
                        <h6>Ćwiczenia w tym treningu:</h6>
                        <div class="list-group">
                `;

                workoutExercises.forEach(exercise => {
                    const exerciseType = this.exerciseTypes?.find(et => et.id === exercise.exerciseTypeId);
                    exercisesHtml += `
                        <div class="list-group-item d-flex justify-content-between align-items-center">
                            <div>
                                <strong>${exerciseType?.name || 'Nieznane ćwiczenie'}</strong><br>
                                <small>${exercise.sets} serii × ${exercise.reps} powtórzeń, ${exercise.weight} kg</small>
                            </div>
                            <button class="btn btn-sm btn-outline-danger" onclick="Workouts.deleteExercise(${exercise.id})">Usuń</button>
                        </div>
                    `;
                });

                exercisesHtml += `</div></div>`;
            }

            const workoutCard = `
                <div class="col-md-6 mb-4">
                    <div class="card h-100">
                        <div class="card-body">
                            <h5 class="card-title">Trening #${workout.id}</h5>
                            <p class="card-text">
                                <strong>Rozpoczęcie:</strong> ${startDate}<br>
                                <strong>Zakończenie:</strong> ${endDate}<br>
                                <strong>Czas trwania:</strong> ${duration} minut
                            </p>
                            ${workout.notes ? `<p class="card-text"><strong>Notatki:</strong> ${workout.notes}</p>` : ''}
                            ${exercisesHtml}
                            <div class="mt-3">
                                <button class="btn btn-sm btn-outline-primary" onclick="Workouts.editWorkout(${workout.id})">Edytuj</button>
                                <button class="btn btn-sm btn-outline-danger" onclick="Workouts.deleteWorkout(${workout.id})">Usuń</button>
                                <button class="btn btn-sm btn-outline-success" onclick="Workouts.addExercise(${workout.id})">Dodaj Ćwiczenie</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            container.innerHTML += workoutCard;
        });
    }

    static addExercise(workoutId) {
        const modal = new bootstrap.Modal(document.getElementById('exerciseModal'));
        const exerciseSelect = document.getElementById('exerciseTypeSelect');

        document.getElementById('currentWorkoutId').value = workoutId;

        exerciseSelect.innerHTML = '<option value="">-- Wybierz ćwiczenie --</option>';
        if (this.exerciseTypes && this.exerciseTypes.length > 0) {
            this.exerciseTypes.forEach(exercise => {
                const option = document.createElement('option');
                option.value = exercise.id;
                option.textContent = exercise.name;
                exerciseSelect.appendChild(option);
            });
        }

        document.getElementById('exerciseWeight').value = '';
        document.getElementById('exerciseSets').value = '3';
        document.getElementById('exerciseReps').value = '10';

        modal.show();
    }

    static async saveExercise() {
        const workoutId = document.getElementById('currentWorkoutId').value;
        const exerciseTypeId = document.getElementById('exerciseTypeSelect').value;
        const weight = document.getElementById('exerciseWeight').value;
        const sets = document.getElementById('exerciseSets').value;
        const reps = document.getElementById('exerciseReps').value;

        if (!exerciseTypeId || !weight || !sets || !reps) {
            alert('Proszę wypełnić wszystkie wymagane pola');
            return;
        }

        const performedExercise = {
            workoutId: parseInt(workoutId),
            exerciseTypeId: parseInt(exerciseTypeId),
            weight: parseFloat(weight),
            sets: parseInt(sets),
            reps: parseInt(reps)
        };

        try {
            const response = await fetch('/api/performed-exercises', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(performedExercise)
            });

            if (response.ok) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('exerciseModal'));
                modal.hide();
                this.loadWorkouts();
                this.showSuccess('Ćwiczenie dodane pomyślnie do treningu!');
            } else {
                this.displayError('Nie udało się dodać ćwiczenia do treningu');
            }
        } catch (error) {
            console.error('Failed to save exercise:', error);
            this.displayError('Nie udało się dodać ćwiczenia do treningu');
        }
    }

    static async deleteExercise(exerciseId) {
        if (!confirm('Czy na pewno chcesz usunąć to ćwiczenie z treningu?')) {
            return;
        }

        try {
            const response = await fetch(`/api/performed-exercises/${exerciseId}`, {
                method: 'DELETE'
            });

            if (response.ok) {
                this.loadWorkouts();
                this.showSuccess('Ćwiczenie usunięte pomyślnie z treningu!');
            } else {
                this.displayError('Nie udało się usunąć ćwiczenia z treningu');
            }
        } catch (error) {
            console.error('Failed to delete exercise:', error);
            this.displayError('Nie udało się usunąć ćwiczenia z treningu');
        }
    }

    static async saveWorkout() {
        const id = document.getElementById('workoutId').value;
        const startDate = document.getElementById('startDate').value;
        const endDate = document.getElementById('endDate').value;
        const notes = document.getElementById('workoutNotes').value;

        if (!startDate || !endDate) {
            alert('Proszę wypełnić datę rozpoczęcia i zakończenia');
            return;
        }

        const workout = {
            startDate: new Date(startDate).toISOString(),
            endDate: new Date(endDate).toISOString(),
            notes: notes
        };

        try {
            let response;
            if (id) {
                response = await fetch(`/api/workouts/${id}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(workout)
                });
            } else {
                response = await fetch('/api/workouts', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(workout)
                });
            }

            if (response.ok) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('workoutModal'));
                modal.hide();
                this.loadWorkouts();
                this.showSuccess(`Trening ${id ? 'zaktualizowany' : 'utworzony'} pomyślnie!`);
            } else {
                this.displayError(`Nie udało się ${id ? 'zaktualizować' : 'utworzyć'} treningu`);
            }
        } catch (error) {
            console.error('Failed to save workout:', error);
            this.displayError(`Nie udało się ${id ? 'zaktualizować' : 'utworzyć'} treningu`);
        }
    }

    static async editWorkout(id) {
        try {
            const response = await fetch(`/api/workouts/${id}`);
            if (response.ok) {
                const workout = await response.json();
                this.openModal(workout);
            } else {
                this.displayError('Nie udało się załadować treningu do edycji');
            }
        } catch (error) {
            console.error('Failed to load workout:', error);
            this.displayError('Nie udało się załadować treningu do edycji');
        }
    }

    static async deleteWorkout(id) {
        if (!confirm('Czy na pewno chcesz usunąć ten trening?')) {
            return;
        }

        try {
            const response = await fetch(`/api/workouts/${id}`, {
                method: 'DELETE'
            });

            if (response.ok) {
                this.loadWorkouts();
                this.showSuccess('Trening usunięty pomyślnie!');
            } else {
                this.displayError('Nie udało się usunąć treningu');
            }
        } catch (error) {
            console.error('Failed to delete workout:', error);
            this.displayError('Nie udało się usunąć treningu');
        }
    }

    static openModal(workout = null) {
        const modal = new bootstrap.Modal(document.getElementById('workoutModal'));
        const modalTitle = document.getElementById('modalTitle');
        const workoutId = document.getElementById('workoutId');
        const startDate = document.getElementById('startDate');
        const endDate = document.getElementById('endDate');
        const workoutNotes = document.getElementById('workoutNotes');

        const now = new Date();
        const nowString = now.toISOString().slice(0, 16);

        if (workout) {
            modalTitle.textContent = 'Edytuj Trening';
            workoutId.value = workout.id;
            startDate.value = new Date(workout.startDate).toISOString().slice(0, 16);
            endDate.value = new Date(workout.endDate).toISOString().slice(0, 16);
            workoutNotes.value = workout.notes || '';
        } else {
            modalTitle.textContent = 'Dodaj Nowy Trening';
            workoutId.value = '';
            startDate.value = nowString;
            endDate.value = nowString;
            workoutNotes.value = '';
        }

        modal.show();
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
    Workouts.init();
});