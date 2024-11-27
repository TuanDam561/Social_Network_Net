using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Net6_Social_Net.Data;
using Net7_Social_Net;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ SignalR
builder.Services.AddSignalR();

// Thêm dịch vụ MVC
builder.Services.AddControllersWithViews();

// Cấu hình DbContext với chuỗi kết nối từ appsettings.json
builder.Services.AddDbContext<SocialNetworkContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Cấu hình bộ nhớ cache cho Session
builder.Services.AddDistributedMemoryCache();

// Cấu hình Session với thời gian timeout và thuộc tính cookie
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo cookie session được gửi trong mọi tình huống
});

// Đăng nhập với Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

var app = builder.Build();

// Kiểm tra môi trường để xử lý lỗi và bảo mật
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kích hoạt Session trước khi Authorization
app.UseSession();

// Cấu hình Authorization
app.UseAuthorization();

// Đăng ký SignalR Hub
app.MapHub<CommentHub>("/commentHub");  

// Định nghĩa route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();




