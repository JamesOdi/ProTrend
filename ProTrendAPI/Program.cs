global using ProTrendAPI.Services.UserSevice;
global using ProTrendAPI.Models;
global using ProTrendAPI.Models.Posts;
global using ProTrendAPI.Models.Response;
global using ProTrendAPI.Models.User;
using ProTrendAPI.Services;
using ProTrendAPI.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(p => p.AddPolicy(Constants.CORS, builder =>
{
    builder.AllowCredentials().SetIsOriginAllowed(origin => true).AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.Configure<DBSettings>(builder.Configuration.GetSection("DBConnection"));
builder.Services.AddSingleton<RegistrationService>();
builder.Services.AddSingleton<PostsService>();
builder.Services.AddSingleton<ProfileService>();
builder.Services.AddSingleton<CategoriesService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<TagsService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.ConsentCookie.IsEssential = true;
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
    
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else
{
    app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    HttpOnly = HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseStaticFiles();

app.UseAuthentication();

app.UseCors(Constants.CORS);

app.UseAuthorization();

app.MapControllers();

app.Run();
