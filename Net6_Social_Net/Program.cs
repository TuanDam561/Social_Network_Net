﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Net6_Social_Net.Data;
using Net7_Social_Net;
using Net7_Social_Net.Class;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ SignalR
builder.Services.AddSignalR();
// Thêm dịch vụ MVC
builder.Services.AddControllersWithViews();

// Cấu hình DbContext cho DatabaseDoAnContext
builder.Services.AddDbContext<SocialNetworkContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);
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
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value!;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value!;
});


//Cấu hình SMTP
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();


// Cấu hình bộ nhớ cache cho Session
builder.Services.AddDistributedMemoryCache();

// Cấu hình Session với thời gian timeout và thuộc tính cookie
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo cookie session được gửi trong mọi tình huống
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
app.MapHub<ChatHub>("/chathub");
// Định nghĩa route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();




