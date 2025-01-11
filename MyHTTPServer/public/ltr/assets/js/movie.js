const slides = document.querySelectorAll('.carousel-slide');

slides.forEach((slide) => {
    const video = slide.querySelector('video');
    const playPauseButton = slide.querySelector('.play-pause');
    const muteUnmuteButton = slide.querySelector('.mute-unmute');

    playPauseButton.addEventListener('click', () => {
        if (video.paused) {
            video.play();
            playPauseButton.textContent = '⏸️';
        } else {
            video.pause();
            playPauseButton.textContent = '▶️';
        }
    });

    muteUnmuteButton.addEventListener('click', () => {
        video.muted = !video.muted;
        muteUnmuteButton.textContent = video.muted ? '🔇' : '🔊';
    });
});