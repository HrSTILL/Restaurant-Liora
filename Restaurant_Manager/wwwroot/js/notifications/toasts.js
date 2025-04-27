console.log('toasts.js is loaded!');

let notifications = document.querySelector('.notifications');
function createToast(type, icon, title, text) {
    let newToast = document.createElement('div');
    newToast.innerHTML = `
        <div class="lio-toast ${type}">
            <i class="${icon}"></i>
            <div class="content">
                <div class="title">${title}</div>
                <span>${text}</span>
            </div>
            <i class="fa-solid fa-xmark" onclick="(this.parentElement).remove()" style="cursor:pointer"></i>
        </div>
    `;
    notifications.appendChild(newToast);
    setTimeout(() => newToast.remove(), 5000);
}

