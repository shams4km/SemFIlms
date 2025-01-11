// Пример функционала для интерактивности
document.addEventListener('DOMContentLoaded', function () {
    console.log('Сайт загружен');

    // Пример добавления события
    const movieCards = document.querySelectorAll('.movie-card');
    movieCards.forEach(card => {
        card.addEventListener('click', () => {
            alert('Вы выбрали фильм!');
        });
    });
});
