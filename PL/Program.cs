using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Interfaces;
using DAL.Repositories;
// using BLL.Services; // Розкоментуємо пізніше

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Реєструємо універсальний репозиторій для всіх типів сутностей
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// builder.Services.AddScoped<ILotService, LotService>();
// builder.Services.AddScoped<IAuctionService, AuctionService>();

var app = builder.Build();

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


