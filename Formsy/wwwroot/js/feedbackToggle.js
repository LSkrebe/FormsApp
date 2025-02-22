document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".feedback-toggle").forEach(button => {
        button.addEventListener("click", function () {
            const formId = this.getAttribute("data-form-id");
            const answerDiv = document.getElementById(`feedback-${formId}`);
            if (answerDiv.style.display === "none") {
                answerDiv.style.display = "block";
            } else {
                answerDiv.style.display = "none";
            }
        });
    });
});
