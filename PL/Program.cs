using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Interfaces;
using DAL.Repositories;
using DAL.Services;
using BLL.Interfaces;
using BLL.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Отримуємо рядок підключення з appsettings.json і підключаємо базу даних
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlServer(connectionString));

// Реєструємо AutoMapper
builder.Services.AddAutoMapper(new[] { typeof(BLL.MappingProfile).Assembly });

// Реєструємо шари DAL (робота з базою)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<SeedService>();

// Реєструємо шари BLL (бізнес-логіка)
builder.Services.AddScoped<ILotService, LotService>();
builder.Services.AddScoped<IAuctionService, AuctionService>();

var app = builder.Build();

// ДОДАНО: Seed тестових даних при запуску (лише в режимі Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
        await seedService.SeedAsync();
    }
}

// ДОДАНО: Middleware для глобального перехоплення помилок
app.UseMiddleware<PL.Middlewares.ExceptionHandlingMiddleware>();

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