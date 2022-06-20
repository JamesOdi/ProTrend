using ProTrendAPI.Services;
using ProTrendAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DBSettings>(builder.Configuration.GetSection("DBConnection"));

builder.Services.AddSingleton<RegistrationService>();
builder.Services.AddSingleton<PostsService>();
builder.Services.AddSingleton<UserProfileService>();
builder.Services.AddSingleton<CategoriesService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
//app.UseMiddleware<UserManagementMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
