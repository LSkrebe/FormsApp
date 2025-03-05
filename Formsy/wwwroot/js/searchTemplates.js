document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchInput");
    const cards = document.querySelectorAll(".template-card");

    searchInput.addEventListener("input", function () {
        let filter = this.value.toLowerCase();

        cards.forEach(card => {
            let searchText = card.getAttribute("data-search");
            card.style.display = searchText.includes(filter) ? "block" : "none";
        });
    });
});
