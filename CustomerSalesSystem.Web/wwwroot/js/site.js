function showLoader() {
    document.getElementById("loader").style.display = "block";
}
function hideLoader() {
    document.getElementById("loader").style.display = "none";
}
function showSuccess(msg) {
    const el = document.getElementById("successMsg");
    el.textContent = msg;
    el.classList.remove("d-none");
}
function showError(msg) {
    const el = document.getElementById("errorMsg");
    el.textContent = msg;
    el.classList.remove("d-none");
}
