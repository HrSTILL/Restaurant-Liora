// (EN) Setup image slider autoplay | (BG) Настройва автоматичното превъртане на слайдър
document.addEventListener('DOMContentLoaded', () => {
    const slider = document.querySelector('.slider');
    const slides = document.querySelectorAll('.slider img');
    let currentIndex = 0;
    setInterval(() => {
        currentIndex++;
        if (currentIndex >= slides.length) {
            currentIndex = 0;
        }
        const scrollAmount = slides[currentIndex].offsetLeft;
        slider.scrollTo({
            left: scrollAmount,
            behavior: 'smooth'
        });
    }, 15000);
});
