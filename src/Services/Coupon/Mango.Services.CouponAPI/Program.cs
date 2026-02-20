using Mango.Services.CouponAPI.Infrastructure;
using Mango.Services.CouponAPI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<CouponDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddHealthChecks().AddDbContextCheck<CouponDbContext>("coupondb");

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseSerilogRequestLogging();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CouponDbContext>();
    db.Database.Migrate();
}

app.Run();
