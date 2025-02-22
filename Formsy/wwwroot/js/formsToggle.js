document.getElementById("btnCreatedForms").addEventListener("click", function () {
    document.getElementById("createdFormsContainer").style.display = "grid";
    document.getElementById("submittedFormsContainer").style.display = "none";
    document.getElementById("btnCreatedForms").classList.add("active");
    document.getElementById("btnSubmittedForms").classList.remove("active");
});

document.getElementById("btnSubmittedForms").addEventListener("click", function () {
    document.getElementById("submittedFormsContainer").style.display = "grid";
    document.getElementById("createdFormsContainer").style.display = "none";
    document.getElementById("btnSubmittedForms").classList.add("active");
    document.getElementById("btnCreatedForms").classList.remove("active");
});
