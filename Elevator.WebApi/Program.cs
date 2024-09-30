using Elevator.Contracts;
using Elevator.Data.Context;
using Elevator.Data.Repository.Interface;
using Elevator.Data.Repository;
using Elevator.Data.SeedData;
using Elevator.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDistributedMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ElevatorDbContext>(options =>  options.UseSqlServer(connectionString));
builder.Services.AddTransient<ElevatorSeedData>();

builder.Services.AddSession();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IElevatorServices, ElevatorServices>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(DataRepository<>));
builder.Services.AddTransient<IContext, ElevatorDbContext>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSession();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ElevatorSeedData>();
    seeder.SeedElevatorData();
}

app.Run();
