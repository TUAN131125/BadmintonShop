using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Core.Interfaces.Security;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Core.Services;
using BadmintonShop.Data.DbContext;
using BadmintonShop.Data.Repositories;
using BadmintonShop.Data.Repositories.Implementations;
using BadmintonShop.Data.UnitOfWork;
using BadmintonShop.Web.Security;
using BadmintonShop.Web.Services;
using BadmintonShop.Web.Services.Payments;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURATION & DATABASE
// ==========================================
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddSession();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ==========================================
// 2. DEPENDENCY INJECTION
// ==========================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IPromotionService, PromotionService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<IPasswordHasher, AspNetPasswordHasher>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddScoped<CurrencyService>();
builder.Services.AddScoped<IPaymentService, PaypalPaymentService>();
builder.Services.AddSingleton(x =>
    new PaypalClient(
        builder.Configuration["PaypalOptions:ClientId"],
        builder.Configuration["PaypalOptions:ClientSecret"],
        builder.Configuration["PaypalOptions:Mode"]
    )
);

// ==========================================
// 3. AUTHENTICATION (C?U HÌNH 2 COOKIES)
// ==========================================

builder.Services.AddAuthentication(options =>
{
    // M?c ??nh là Customer (cho trang Home, Product...)
    options.DefaultScheme = "CustomerAuth";
    options.DefaultChallengeScheme = "CustomerAuth";
})
// 1. C?U HÌNH COOKIE CHO KHÁCH HÀNG
.AddCookie("CustomerAuth", options =>
{
    options.Cookie.Name = ".BadmintonShop.Customer"; // Tên Cookie riêng
    options.LoginPath = "/Account/Login";            // Trang ??ng nh?p khách
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
})
// 2. C?U HÌNH COOKIE CHO ADMIN (??C L?P)
.AddCookie("AdminAuth", options =>
{
    options.Cookie.Name = ".BadmintonShop.Admin";    // Tên Cookie riêng
    options.LoginPath = "/Admin/Auth/Login";         // Trang ??ng nh?p Admin
    options.LogoutPath = "/Admin/Auth/Logout";
    options.AccessDeniedPath = "/Admin/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ==========================================
// 4. PIPELINE
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();