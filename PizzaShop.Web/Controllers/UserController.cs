using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Model.Strings;
using Newtonsoft.Json;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Service.Attributes;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Web.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;

    private readonly ITokenDataService _tokenDataService;

    private readonly IMailService _mailService;

    private readonly IRolePermissionService _rolePermission;
    public UserController(IUserService userService, ITokenDataService tokenDataService, IMailService mailService, IRolePermissionService rolePermissionService)
    {
        _userService = userService;

        _tokenDataService = tokenDataService;

        _mailService = mailService;

        _rolePermission = rolePermissionService;
    }

    public IActionResult ChangePassword(bool useOrderAppLayout = false)
    {
        ViewBag.UseOrderAppLayout = useOrderAppLayout;
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
    public async Task<IActionResult> Profile(bool useOrderAppLayout = false)
    {
        var token = Request.Cookies["Token"];
        var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token);

        if (string.IsNullOrEmpty(email))
        {
            TempData["ToastrMessage"] = "Session expired! Please log in again.";
            TempData["ToastrType"] = "error";
            return RedirectToAction("Login", "Auth");
        }

        var userViewModel = await _userService.GetUserProfileAsync(email);
        if (userViewModel == null)
        {
            ViewBag.UseOrderAppLayout = useOrderAppLayout;
            return RedirectToAction("AdminDashboard", "Home");
        }

        var allCountries = await _userService.GetAllCountriesAsync();
        var allStates = await _userService.GetStatesByCountryIdAsync(userViewModel.CountryId);
        var allCities = await _userService.GetCitiesByStateIdAsync(userViewModel.StateId);

        ViewBag.Countries = new SelectList(allCountries, "Id", "Name", userViewModel.CountryId);
        ViewBag.States = new SelectList(allStates, "Id", "Name", userViewModel.StateId);
        ViewBag.Cities = new SelectList(allCities, "Id", "Name", userViewModel.CityId);
        ViewBag.UseOrderAppLayout = useOrderAppLayout;

        return View(userViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            var token = Request.Cookies["Token"];
            var (email, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token);

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
        // ViewBag.UseOrderAppLayout = useOrderAppLayout;
        await PopulateDropdowns();
        return View(model);
    }

    [CustomAuthorize("Users", "CanView")]
    public async Task<IActionResult> UserList(string searchString, int pageIndex = 1, int pageSize = 5, string sortOrder = "")
    {
        var token = Request.Cookies["Token"];
        var roleId = await _tokenDataService.GetRoleFromToken(token!);
        var permission = _rolePermission.GetRolePermissionByRoleId(roleId);
        HttpContext.Session.SetString("permission", JsonConvert.SerializeObject(permission));

        var (currentUserEmail, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        var users = _userService.GetUserList(searchString, sortOrder, pageIndex, pageSize, int.Parse(userId));
        var totalUsers = _userService.GetTotalUsers(searchString);
        ViewBag.count = totalUsers;

        ViewData["UsernameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "username_desc" : sortOrder == "username_desc" ? "username_asc" : "username_desc";
        ViewData["RoleSortParam"] = sortOrder == "role_asc" ? "role_desc" : "role_asc";

        var userListPage = new UserPageViewModel
        {
            UserList = users,
            PageIndex = pageIndex,
            PageSize = pageSize,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(totalUsers / (double)pageSize),
            TotalUsers = totalUsers,
            SortOrder = sortOrder
        };

        return View(userListPage);
    }

    [CustomAuthorize("Users", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetUsers(string searchString, int pageIndex, int pageSize, string sortOrder = "")
    {
        var count = _userService.GetTotalUsers(searchString);
        var token = Request.Cookies["Token"];
        var (currentUserEmail, userId, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);

        ViewData["UsernameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "username_asc" : sortOrder == "username_asc" ? "username_desc" : "username_asc";
        ViewData["RoleSortParam"] = sortOrder == "role_asc" ? "role_desc" : "role_asc";

        var userList = _userService.GetUserList(searchString, sortOrder, pageIndex, pageSize, int.Parse(userId));
        var userListPage = new UserPageViewModel
        {
            UserList = userList,
            PageIndex = pageIndex,
            PageSize = pageSize,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(count / (double)pageSize),
            TotalUsers = count,
            SortOrder = sortOrder
        };
        return PartialView("_UserListPartial", userListPage);
    }

    [CustomAuthorize("Users", "CanEdit")]
    public async Task<IActionResult> CreateUser()
    {
        await PopulateDropdowns();
        return View();
    }

    [CustomAuthorize("Users", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserViewModel model)
    {
        if (ModelState.IsValid)
        {

            if (await _userService.UserExistsAsync(model.Email, model.Username))
            {
                ViewBag.UserExistError = "User Already Exist";
                TempData["ToastrMessage"] = "User Already Exist!";
                TempData["ToastrType"] = "error";
                await PopulateDropdowns();
                return View(model);
            }
            var token = Request.Cookies["Token"];
            var (currentUserEmail, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token);
            if (token == null)
            {
                Response.Cookies.Delete("Token");
                Response.Cookies.Delete("UserData");
                TempData["ToastrMessage"] = "Your Session is Expired!";
                TempData["ToastrType"] = "error";
                return RedirectToAction("Login", "Auth");
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
                    return View(model);
                }
                else if (model.ProfileImagePath.Length > maxFileSize)
                {
                    ModelState.AddModelError("ProfileImagePath", "The file size should not exceed 2MB.");
                    TempData["ToastrMessage"] = "The file size should not exceed 2MB.";
                    TempData["ToastrType"] = "error";
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

            await _userService.AddUserAsync(model, int.Parse(id));

            // Send Mail
            string templateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/NewUserEmail.html");
            if (!System.IO.File.Exists(templateFilePath))
            {
                ModelState.AddModelError(string.Empty, "Email body template not found");
            }
            string body = await System.IO.File.ReadAllTextAsync(templateFilePath);
            body = body.Replace("{{Email}}", model.Email);
            body = body.Replace("{{PasswordHash}}", model.Password);
            _mailService.SendMail(model.Email, body, "Account Login Details");

            TempData["ToastrMessage"] = "User Created Successfully";
            TempData["ToastrType"] = "success";

            return RedirectToAction("UserList");
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
        return View(model);

    }

    private async Task PopulateDropdowns()
    {
        var countries = await _userService.GetAllCountriesAsync();
        var roles = await _userService.GetAllRolesAsync();

        ViewBag.Countries = new SelectList(countries, "Id", "Name");
        ViewBag.Roles = new SelectList(roles, "Id", "Name");
    }

    [HttpPost]
    [Route("User/DeleteUser/{id}/{roleId}")]
    [CustomAuthorize("Users", "CanDelete")]
    public IActionResult DeleteUser(int id, int roleId)
    {
        _userService.DeleteUser(id);
        TempData["ToastrMessage"] = "User Deleted Successfully";
        TempData["ToastrType"] = "success";
        return RedirectToAction(nameof(UserList));
    }

    [CustomAuthorize("Users", "CanEdit")]
    public async Task<IActionResult> UpdateUser(int id)
    {
        ViewBag.Roles = new SelectList(await _userService.GetAllRolesAsync(), "Id", "Name");

        var model = await _userService.GetUserByIdForUpdate(id);
        if (model == null)
        {
            return NotFound();
        }

        ViewBag.Roles = new SelectList(await _userService.GetAllRolesAsync(), "Id", "Name");
        ViewBag.Countries = new SelectList(await _userService.GetAllCountriesAsync(), "Id", "Name", model.CountryId);
        return View(model);
    }

    [CustomAuthorize("Users", "CanEdit")]
    [HttpPost]
    public async Task<IActionResult> UpdateUser(UserUpdateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var token = Request.Cookies["Token"];
            var (currentUserEmail, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
            if (token == null)
            {
                Response.Cookies.Delete("Token");
                Response.Cookies.Delete("UserData");
                TempData["ToastrMessage"] = "Your Session is Expired!";
                TempData["ToastrType"] = "error";
                return RedirectToAction("Login", "Auth");
            }
            model.ModifiedBy = int.Parse(id);
            var user = _userService.GetUserById(model.Id);
            if (user.Username != model.Username)
            {
                if (await _userService.UserExistsAsync(model.Email!, model.Username!))
                {
                    ViewBag.UserExistError = "User Already Exist";
                    TempData["ToastrMessage"] = "User Already Exist!";
                    TempData["ToastrType"] = "error";
                    await PopulateDropdowns();
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
                    await PopulateDropdowns();
                    return View(model);
                }
                else if (model.ProfileImagePath.Length > maxFileSize)
                {
                    ModelState.AddModelError("ProfileImagePath", "The file size should not exceed 2MB.");
                    TempData["ToastrMessage"] = "The file size should not exceed 2MB.";
                    TempData["ToastrType"] = "error";
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

            TempData["ToastrMessage"] = "User Updated Successfully";
            TempData["ToastrType"] = "success";
            await _userService.UpdateUserAsync(model);

            return RedirectToAction("UserList");
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
        return View(model);
    }

    [HttpGet]
    public async Task<JsonResult> GetProfileDetail()
    {
        var token = Request.Cookies["Token"];
        var (currentUserEmail, id, isFirstLogin) = await _tokenDataService.GetEmailFromToken(token!);
        var user = await _userService.GetUserByEmailAsync(currentUserEmail);
        if (user != null)
        {
            return Json(new { profileImage = user!.ProfileImage, userName = user.Username });
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetStates(int countryId)
    {
        var states = await _userService.GetStatesByCountryIdAsync(countryId);
        return Json(states);
    }

    [HttpGet]
    public async Task<IActionResult> GetCities(int stateId)
    {
        var cities = await _userService.GetCitiesByStateIdAsync(stateId);
        return Json(cities);
    }
}