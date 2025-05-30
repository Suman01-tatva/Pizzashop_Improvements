function handleAjaxError(xhr, status, error) {
  if (xhr.status === 401) {
    toastr.error("You are not authenticated. Please log in.");
  } else if (xhr.status === 403) {
    toastr.error(
      xhr.responseJSON && xhr.responseJSON.message
        ? xhr.responseJSON.message
        : "You are not authorized to perform this action."
    );
    $(".modal-backdrop").remove();
  } else {
    toastr.error(
      "An error occurred while processing your request. Please try again."
    );
  }
}

$(function () {
  var message = '@Html.Raw(TempData["ToastrMessage"])';
  var type = '@Html.Raw(TempData["ToastrType"])';
  if (message.trim() !== "") {
    switch (type) {
      case "success":
        toastr.success(message);
        break;
      case "error":
        toastr.error(message);
        break;
      case "warning":
        toastr.warning(message);
        break;
      case "info":
        toastr.info(message);
    }
  }
});

const currentPath = window.location.pathname;

const navLinks = document.querySelectorAll(".capsule");

navLinks.forEach((link) => {
  const linkPath = link.getAttribute("href");
  if (currentPath.endsWith(linkPath)) {
    link.classList.add("active-navicon");
  } else {
    link.classList.remove("active-navicon");
  }
});
