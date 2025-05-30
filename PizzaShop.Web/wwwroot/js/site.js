// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function togglePassword(fieldId, icon) {
  var passwordField = document.getElementById(fieldId);
  console.log("clickk");
  if (passwordField.type === "password") {
    passwordField.type = "text";
    icon.classList.remove("fa-eye");
    icon.classList.add("fa-eye-slash");
  } else {
    passwordField.type = "password";
    icon.classList.remove("fa-eye-slash");
    icon.classList.add("fa-eye");
  }
}

const currentPath = window.location.pathname;
const navLinks = sidebar.querySelectorAll(".nav-item");

navLinks.forEach((link) => {
  if (link.getAttribute("href") === currentPath) {
    link.classList.add("active-navicon");
  } else {
    link.classList.remove("active-navicon");
  }
});
