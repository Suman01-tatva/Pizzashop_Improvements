namespace PizzaShop.Service.Implementations;

using System.Diagnostics.Metrics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;
using PizzaShop.Service.Utils;

public class UserService : IUserService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IStateRepository _stateRepository;
    private readonly ICityRepository _cityRepository;

    public UserService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        ICountryRepository countryRepository,
        IStateRepository stateRepository,
        ICityRepository cityRepository)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _countryRepository = countryRepository;
        _stateRepository = stateRepository;
        _cityRepository = cityRepository;
    }

    public async Task<string> ChangePasswordAsync(ChangePasswordViewModel model, string userEmail)
    {
        var user = await _userRepository.GetUserByEmailAsync(userEmail);
        var hashPassword = PasswordUtills.HashPassword(model.CurrentPassword);
        var hasNewPassword = PasswordUtills.HashPassword(model.NewPassword);
        if (user?.Password == hashPassword)
        {
            if (user?.Password == hasNewPassword)
            {
                return "New password must be different from the current password.";
            }
            string changedPassword = PasswordUtills.HashPassword(model.NewPassword);
            user!.Password = changedPassword;
            if (user.IsFirstLogin == true)
            {
                user.IsFirstLogin = false;
            }
            await _userRepository.UpdateUserAsync(user);
            return "success";
        }
        else
        {
            return "current password is incorrect";
        }
    }

    public async Task<ProfileViewModel> GetUserProfileAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        var role = await _roleRepository.GetRoleByIdAsync(user!.RoleId);

        var profileViewModel = new ProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Phone = user.Phone,
            CountryId = user.CountryId,
            StateId = user.StateId,
            CityId = user.CityId,
            Zipcode = user.Zipcode,
            Address = user.Address,
            RoleId = user.RoleId,
            RoleName = role.Name,
            Email = email,
            ProfileImg = user.ProfileImage
        };

        return profileViewModel;
    }

    public async Task UpdateUserProfileAsync(ProfileViewModel model, string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) return;

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.Username!;
        user.Phone = model.Phone;
        user.CityId = model.CityId;
        user.StateId = model.StateId;
        user.CountryId = model.CountryId;
        user.Address = model.Address!.Trim();
        user.Zipcode = model.Zipcode;
        user.ProfileImage = model.ProfileImg;

        await _userRepository.UpdateUserAsync(user);
    }

    public IEnumerable<User> GetUserList(string searchString, string sortOrder, int pageIndex, int pageSize, int userId)
    {
        return _userRepository.GetUserList(searchString, sortOrder, pageIndex, pageSize, userId);
    }

    public int GetTotalUsers(string searchString)
    {
        return _userRepository.GetTotalUsers(searchString);
    }

    public User GetUserById(int id)
    {
        return _userRepository.GetUserById(id);
    }

    public void DeleteUser(int id)
    {
        _userRepository.DeleteUser(id);
    }

    public async Task<UserUpdateViewModel> GetUserByIdForUpdate(int id)
    {
        var user = _userRepository.GetUserById(id);

        var userViewModel = new UserUpdateViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Phone = user.Phone,
            CountryId = user.CountryId,
            StateId = user.StateId,
            CityId = user.CityId,
            Zipcode = user.Zipcode,
            Address = user.Address,
            RoleId = user.RoleId,
            Email = user.Email,
            IsActive = (bool)user.IsActive!,
            ProfileImg = user.ProfileImage
        };

        return userViewModel;
    }

    public async Task UpdateUserAsync(UserUpdateViewModel model)
    {
        var user = await _userRepository.GetUserByEmailAsync(model.Email!);
        if (user == null) return;

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.Username!;
        user.Phone = model.Phone;
        user.CityId = model.CityId;
        user.StateId = model.StateId;
        user.CountryId = model.CountryId;
        user.Address = model.Address!.Trim();
        user.Zipcode = model.Zipcode;
        user.RoleId = model.RoleId;
        user.IsActive = model.IsActive;
        user.ProfileImage = model.ProfileImg;
        user.ModifiedBy = model.ModifiedBy;
        user.IsFirstLogin = model.IsFirstLogin;
        await _userRepository.UpdateUserAsync(user);
    }
    public async Task<List<Country>> GetAllCountriesAsync()
    {
        return await _countryRepository.GetAllCountriesAsync();
    }

    public async Task<List<State>> GetStatesByCountryIdAsync(int? countryId)
    {
        return await _stateRepository.GetStatesByCountryIdAsync(countryId);
    }

    public async Task<List<City>> GetCitiesByStateIdAsync(int? stateId)
    {
        return await _cityRepository.GetCitiesByStateIdAsync(stateId);
    }

    public async Task<bool> UserExistsAsync(string email, string userName)
    {
        var user = await _userRepository.IsExistUser(email, userName);
        return user != null;
    }

    public async Task AddUserAsync(UserViewModel model, int currentUserId)
    {
        var hashPassword = PasswordUtills.HashPassword(model.Password!);

        var newUser = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.Username!,
            Phone = model.Phone,
            CountryId = model.CountryId,
            StateId = model.StateId,
            CityId = model.CityId,
            Address = model.Address!.Trim(),
            Zipcode = model.Zipcode,
            RoleId = model.RoleId,
            ProfileImage = model.ProfileImg,
            Email = model.Email!,
            Password = hashPassword,
            CreatedBy = currentUserId,
        };

        await _userRepository.AddUserAsync(newUser);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _roleRepository.GetAllRolesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        return user;
    }
}