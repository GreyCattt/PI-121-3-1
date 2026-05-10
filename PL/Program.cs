using Microsoft.EntityFrameworkCore;
// using DAL.Data; // Розкоментуємо це пізніше, коли створимо AuctionDbContext
// using BLL.Services; // Розкоментуємо пізніше

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Налаштовуємо Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
/*
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlServer(connectionString));
*/
// ---------------------------------------------------------
// 2. РЕЄСТРАЦІЯ DEPENDENCY INJECTION (DAL та BLL)
// ---------------------------------------------------------
// Тут ми будемо реєструвати наші репозиторії та сервіси. 
// Залишаємо ці рядки закоментованими як нагадування:

// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
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


