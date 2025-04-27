// (EN) Setup staff search and row selection | (BG) Настройка на търсене и избор на служители
document.addEventListener("DOMContentLoaded", function () {
    const staffSearchInput = document.getElementById("staffSearchInput");
    if (staffSearchInput) {
        staffSearchInput.addEventListener("input", function () {
            performSearch("/Admin/ManageStaff", staffSearchInput.value);
        });
    }
// (EN) Perform live search request | (BG) Изпълнява търсене в реално време
    function performSearch(url, searchTerm) {
        fetch(`${url}?search=${encodeURIComponent(searchTerm)}&page=1`)
            .then(response => response.text())
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, "text/html");

                document.querySelector("#userTable").innerHTML = doc.querySelector("#userTable").innerHTML;
                document.querySelector(".pagination").innerHTML = doc.querySelector(".pagination").innerHTML;

                reattachRowClickEvents();
            })
            .catch(error => console.error('Search failed:', error));
    }
// (EN) Restore buttons for table rows | (BG) Възстановява бутоните за редовете в таблицата
    function reattachRowClickEvents() {
        const rows = document.querySelectorAll("#userTable tbody tr");
        rows.forEach(row => {
            row.addEventListener("click", () => {
                rows.forEach(r => r.classList.remove("selected"));
                row.classList.add("selected");
                selectedUserId = row.getAttribute("data-id");

                document.getElementById("editBtn").disabled = false;
                document.getElementById("deleteBtn").disabled = false;
            });
        });
    }
});
