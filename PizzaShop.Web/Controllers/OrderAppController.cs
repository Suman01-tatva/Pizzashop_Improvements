using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;
[CustomAuthorize]
public class OrderAppController : Controller
{

    private readonly IRolePermissionService _rolePermissionService;
    private readonly ITokenDataService _tokenDataService;
    private readonly IUserService _userService;
    public OrderAppController(IRolePermissionService rolePermissionService, ITokenDataService tokenDataService, IUserService userService)
    {
        _rolePermissionService = rolePermissionService;
        _tokenDataService = tokenDataService;
        _userService = userService;
    }

    public IActionResult ChangePassword()
    {
        // ViewBag.UseOrderAppLayout = useOrderAppLayout;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var token = Request.Cookies["Token"];
            var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
            string isPasswordChanged = await _userService.ChangePasswordAsync(model, email);
            if (isPasswordChanged == "success")
            {
                TempData["ToastrMessage"] = "Password Changed Successfully";
                TempData["ToastrType"] = "success";
                Response.Cookies.Delete("Token");
                Response.Cookies.Delete("UserData");
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                TempData["ToastrMessage"] = isPasswordChanged;
                TempData["ToastrType"] = "error";
                ModelState.AddModelError("", "Please Enter valid password");
                return View();
            }
        }
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var token = Request.Cookies["Token"];
        var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);

        if (string.IsNullOrEmpty(email))
        {
            TempData["ToastrMessage"] = "Session expired! Please log in again.";
            TempData["ToastrType"] = "error";
            return RedirectToAction("Login", "Auth");
        }

        var userViewModel = await _userService.GetUserProfileAsync(email);
        if (userViewModel == null)
        {
            // ViewBag.UseOrderAppLayout = useOrderAppLayout;
            return RedirectToAction("AdminDashboard", "Home");
        }

        var allCountries = await _userService.GetAllCountriesAsync();
        var allStates = await _userService.GetStatesByCountryIdAsync(userViewModel.CountryId);
        var allCities = await _userService.GetCitiesByStateIdAsync(userViewModel.StateId);

        ViewBag.Countries = new SelectList(allCountries, "Id", "Name", userViewModel.CountryId);
        ViewBag.States = new SelectList(allStates, "Id", "Name", userViewModel.StateId);
        ViewBag.Cities = new SelectList(allCities, "Id", "Name", userViewModel.CityId);
        // ViewBag.UseOrderAppLayout = useOrderAppLayout;

        return View(userViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            var token = Request.Cookies["Token"];
            var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);

            var user = _userService.GetUserById(int.Parse(id));
            if (user.Username != model.Username)
            {
                if (await _userService.UserExistsAsync(model.Email!, model.Username!))
                {
                    ViewBag.UserExistError = "User Already Exist";
                    TempData["ToastrMessage"] = "User Already Exist!";
                    TempData["ToastrType"] = "error";
                    await PopulateDropdowns();
                    // ViewBag.UseOrderAppLayout = useOrderAppLayout;
                    return View(model);
                }
            }

            // Upload img
            string ProfileImagePath = null;
            if (model.ProfileImagePath != null && model.ProfileImagePath.Length > 0)
            {
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                var extension = Path.GetExtension(model.ProfileImagePath.FileName).ToLower();
                var maxFileSize = 2 * 1024 * 1024;
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProfileImagePath", "Please upload an image file (.png, .jpg, .jpeg).");
                    TempData["ToastrMessage"] = "Please upload an image file in (.png, .jpg, .jpeg) format.";
                    TempData["ToastrType"] = "error";
                    // ViewBag.UseOrderAppLayout = useOrderAppLayout;
                    return View(model);
                }
                else if (model.ProfileImagePath.Length > maxFileSize)
                {
                    ModelState.AddModelError("ProfileImagePath", "The file size should not exceed 2MB.");
                    TempData["ToastrMessage"] = "The file size should not exceed 2MB.";
                    TempData["ToastrType"] = "error";
                    // ViewBag.UseOrderAppLayout = useOrderAppLayout;
                    return View(model);
                }
                else
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/ProfileImages");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var filename = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(folderPath, filename);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfileImagePath.CopyTo(stream);
                    }
                    ProfileImagePath = "/ProfileImages/" + filename;
                }
            }

            if (ProfileImagePath != null)
                model.ProfileImg = ProfileImagePath;

            await _userService.UpdateUserProfileAsync(model, email);

            TempData["ToastrMessage"] = "Profile Updated Successfully";
            TempData["ToastrType"] = "success";
        }
        else
        {
            var errorMessage = "";
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    errorMessage += $"{state.Key}: {state.Value.Errors.First().ErrorMessage}; ";
                }
            }
            TempData["ToastrMessage"] = errorMessage;
            TempData["ToastrType"] = "error";
        }
        await PopulateDropdowns();
        return RedirectToAction("Table", "OrderAppTable");
    }


    private async Task PopulateDropdowns()
    {
        var countries = await _userService.GetAllCountriesAsync();
        var roles = await _userService.GetAllRolesAsync();

        ViewBag.Countries = new SelectList(countries, "Id", "Name");
        ViewBag.Roles = new SelectList(roles, "Id", "Name");
    }
}
