document.addEventListener("DOMContentLoaded", function () {
    const userSearchInput = document.getElementById("userSearchInput");

    if (userSearchInput) {
        userSearchInput.addEventListener("input", function () {
            performSearch("/Admin/ManageUsers", userSearchInput.value);
        });
    }

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

    function reattachRowClickEvents() {
        const rows = document.querySelectorAll("#userTable tbody tr");
        rows.forEach(row => {
            row.addEventListener("click", () => {
                rows.forEach(r => r.classList.remove("selected"));
                row.classList.add("selected");
                selectedUserId = row.getAttribute("data-id");

                document.getElementById("editBtn").disabled = false;
                document.getElementById("deleteBtn").disabled = false;
                document.getElementById("detailsBtn").disabled = false;
            });
        });
    }
});
