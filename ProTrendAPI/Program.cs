global using ProTrendAPI.Services.UserSevice;
global using ProTrendAPI.Models;
global using ProTrendAPI.Models.Posts;
global using ProTrendAPI.Models.Response;
global using ProTrendAPI.Models.User;
using ProTrendAPI.Services;
using ProTrendAPI.Settings;
using ProTrendAPI.Services.Network;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(p => p.AddPolicy(Constants.CORS, builder =>
{
    builder.SetIsOriginAllowed(host => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
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

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddAuthentication(Constants.AUTH).AddCookie(Constants.AUTH, options =>
{
    options.Cookie.Name = Constants.AUTH;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Domain = "protrend.herokuapp.com";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseRouting();

app.UseStaticFiles();

app.UseAuthentication();

app.UseCors(Constants.CORS);

app.UseAuthorization();

app.MapControllers();

app.Run();
