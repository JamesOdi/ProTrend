global using ProTrendAPI.Services.UserSevice;
global using ProTrendAPI.Models;
global using ProTrendAPI.Models.Posts;
global using ProTrendAPI.Models.Response;
global using ProTrendAPI.Models.User;
using ProTrendAPI.Services;
using ProTrendAPI.Settings;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddCors(p => p.AddPolicy(Constants.CORS, builder =>
{
    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
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

builder.Services.AddAuthentication(Constants.AUTH).AddCookie(Constants.AUTH, options =>
{
    options.Cookie.Name = Constants.AUTH;
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
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseCors(Constants.CORS);

app.UseAuthorization();

app.MapControllers();

app.Run();
