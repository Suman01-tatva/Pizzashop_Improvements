using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pizzashop.Service.Implementations;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Implementations;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Implementations;
using PizzaShop.Service.Interfaces;
using PizzaShop.Web;
using PizzaShop.Web.Hubs;
using PizzaShop.Web.Middleware;
using PizzaShop.Web.Notifiers.cs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PizzashopContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

DependencyInjection.RegisterServices(builder.Services, builder.Configuration.GetConnectionString("DatabaseConnection")!);

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepostory>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenDataService, TokenDataService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IStateRepository, StateRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
builder.Services.AddScoped<IMenuItemsRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMenuModifierGroupRepository, MenuModifierGroupRepository>();
builder.Services.AddScoped<IMenuModifierRepository, MenuModifierRepository>();
builder.Services.AddScoped<IMenuModifierService, MenuModifierService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepositoy>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IKotNotifier, KotNotifier>();

builder.Services.AddScoped<IOrderAppMenuService, OrderAppMenuService>();
builder.Services.AddScoped<IOrderedItemModifierMappingRepository, OrderedItemModifierMappingRepoSitory>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDataProtection().SetApplicationName("PizzaShop");
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Home/";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("Token"))
            {
                context.Token = context.Request.Cookies["Token"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSignalR();

builder.Services.AddAuthorization();

// // Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("SaveOrderPolicy", context =>
{
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    Console.WriteLine($"Rate limiting for IP: {ip}");
    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: ip,
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePages(async context =>
   {
       if (context.HttpContext.Response.StatusCode == 404)
       {
           context.HttpContext.Response.Redirect("/Home/NotFound");
       }
   });
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapHub<KotHub>("/kotHub");
app.UseRouting();

// app.UseRateLimiter();

// Add the authentication middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseMiddleware<IsFirstLoginMiddleware>(builder.Configuration["Jwt:Key"], "Token");
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();