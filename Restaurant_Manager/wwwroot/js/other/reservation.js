// (EN) Setup reservation modal flow | (BG) Настройва процеса на резервация
document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("reservationModal");
    const openBtn = document.querySelector(".open-modal-button");
    const closeBtn = document.querySelector(".modal-close-btn");
    let selectedDate = null;
    let selectedTime = null;
    let selectedPeople = null;
    if (openBtn) {
        openBtn.addEventListener("click", () => {
            modal.classList.add("show");
            showStep(".step-month");
        });
    }
    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            modal.classList.remove("show");
        });
    }
// (EN) Show specific reservation step | (BG) Показва конкретна стъпка за резервация
    function showStep(selector) {
        document.querySelectorAll(".modal-step").forEach(step => step.style.display = "none");
        document.querySelector(selector).style.display = "block";
    }
    document.querySelectorAll(".step-month .modal-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            const month = parseInt(btn.dataset.month);
            const year = new Date().getFullYear();
            generateDatesGrid(year, month);
            showStep(".step-day");
        });
    });
// (EN) Generate date grid for selected month | (BG) Генерира мрежа от дати за избрания месец
    function generateDatesGrid(year, month) {
        const grid = document.getElementById("dateGrid");
        grid.innerHTML = "";
        const daysInMonth = new Date(year, month, 0).getDate();
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        for (let day = 1; day <= daysInMonth; day++) {
            const date = new Date(year, month - 1, day);
            date.setHours(0, 0, 0, 0);
            const btn = document.createElement("button");
            btn.className = "modal-btn";
            btn.textContent = date.toLocaleDateString("en-US", { month: "short", day: "numeric" });
            btn.dataset.date = date.toISOString().split("T")[0];
            if (date < today) {
                btn.disabled = true;
                btn.classList.add("disabled");
            } else {
                btn.addEventListener("click", () => {
                    selectedDate = date;
                    generateTimeGrid(date);
                    showStep(".step-time");
                });
            }
            grid.appendChild(btn);
        }
    }
// (EN) Generate time grid for selected date | (BG) Генерира мрежа от часове за избраната дата
    function generateTimeGrid(date) {
        const grid = document.getElementById("timeGrid");
        grid.innerHTML = "";
        const now = new Date();
        const isToday = date.toDateString() === now.toDateString();
        const duration = document.querySelector('select[name="DurationType"]').value;
        const allTimes = [
            "08:00 AM", "08:30 AM", "09:00 AM", "09:30 AM", "10:00 AM", "10:30 AM",
            "11:00 AM", "11:30 AM", "12:00 PM", "12:30 PM", "01:00 PM", "01:30 PM",
            "02:00 PM", "02:30 PM", "03:00 PM", "03:30 PM", "04:00 PM", "04:30 PM",
            "05:00 PM", "05:30 PM", "06:00 PM", "06:30 PM", "07:00 PM", "07:30 PM",
            "08:00 PM", "08:30 PM", "09:00 PM", "09:30 PM", "10:00 PM", "10:30 PM",
            "11:00 PM", "11:30 PM"
        ];
        const allowedTimes = {
            "Standard": allTimes,
            "Extended": allTimes.slice(0, -1),
            "ExtendedPlus": allTimes.slice(0, 24)
        };
        allowedTimes[duration].forEach(timeStr => {
            const btn = document.createElement("button");
            btn.className = "modal-btn";
            btn.textContent = timeStr;
            const timeParts = timeStr.match(/(\d+):(\d+) (AM|PM)/);
            let hour = parseInt(timeParts[1]);
            const minute = parseInt(timeParts[2]);
            const ampm = timeParts[3];
            if (ampm === "PM" && hour !== 12) hour += 12;
            if (ampm === "AM" && hour === 12) hour = 0;
            const slot = new Date(date.getFullYear(), date.getMonth(), date.getDate(), hour, minute);
            if (isToday && slot <= now) {
                btn.disabled = true;
                btn.classList.add("disabled");
            } else {
                btn.addEventListener("click", () => {
                    selectedTime = timeStr;
                    showStep(".step-people");
                    generatePeopleOptions();
                });
            }
            grid.appendChild(btn);
        });
    }
// (EN) Generate people options | (BG) Генерира опции за хора
    function generatePeopleOptions() {
        const grid = document.getElementById("peopleGrid");
        grid.innerHTML = "";

        for (let i = 1; i <= 12; i++) {
            const btn = document.createElement("button");
            btn.className = "modal-btn";
            btn.textContent = `${i} ${i === 1 ? "person" : "people"}`;
            btn.addEventListener("click", () => {
                selectedPeople = i;
                applySelectionToForm();
                closeReservationModal();
            });
            grid.appendChild(btn);
        }
    }
// (EN) Apply selected date, time, and people to the form | (BG) Прилага избраната дата, час и хора към формуляра
    function applySelectionToForm() {
        if (!selectedDate || !selectedTime || !selectedPeople) return;
        const year = selectedDate.getFullYear();
        const month = String(selectedDate.getMonth() + 1).padStart(2, '0');
        const day = String(selectedDate.getDate()).padStart(2, '0');
        const dateStr = `${year}-${month}-${day}`;
        const parts = selectedTime.match(/(\d+):(\d+) (AM|PM)/);
        let hour = parseInt(parts[1]);
        const minute = parseInt(parts[2]);
        const ampm = parts[3];
        if (ampm === "PM" && hour < 12) hour += 12;
        if (ampm === "AM" && hour === 12) hour = 0;
        const timeStr = `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}:00`;
        document.getElementById("ReservationDate").value = dateStr;
        document.getElementById("ReservationHour").value = timeStr;
        document.getElementById("NumberOfPeople").value = selectedPeople;
        document.getElementById("reservationSummary").innerHTML = `
            <p><strong>Date:</strong> ${dateStr}</p>
            <p><strong>Time:</strong> ${selectedTime}</p>
            <p><strong>People:</strong> ${selectedPeople}</p>
        `;
    }
// (EN) Close reservation modal | (BG) Затваря модал за резервация
    function closeReservationModal() {
        const modal = document.getElementById("reservationModal");
        if (modal) {
            modal.classList.remove("show");
            modal.style.display = "none";
        }
    }
    document.querySelector(".step-day .back-btn").addEventListener("click", () => showStep(".step-month"));
    document.querySelector(".step-time .back-btn").addEventListener("click", () => showStep(".step-day"));
    document.querySelector(".step-people .back-btn").addEventListener("click", () => showStep(".step-time"));
});
// (EN) Close overlap modal for reservations | (BG) Затваря модала за припокриване на резервации
function closeOverlapModal() {
    const modal = document.getElementById("overlapModal");
    if (modal) {
        modal.classList.remove("show");
        setTimeout(() => {
            modal.style.display = "none";
        }, 200);
    }
}
// (EN) Show overlap modal for reservations | (BG) Показва модала за припокриване на резервации
window.addEventListener("DOMContentLoaded", () => {
    const hasOverlap = '@TempData["OverlapError"]' !== '';
    const modal = document.getElementById("overlapModal");
    if (hasOverlap && modal) {
        modal.style.display = "flex";
    }
});
