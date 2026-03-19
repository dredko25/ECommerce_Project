using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.Application.Services;
using ECommerce_Project.DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddDbContext<ECommerceDbContext>(
    options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"));
    });


builder.Services.AddAutoMapper(cfg => {}, AppDomain.CurrentDomain.GetAssemblies());

// Add product service to the dependency injection container
builder.Services.AddScoped<IProductService, ProductService>();

// ── Сервіси — реєструємо інтерфейс + реалізацію ──
// Scoped = один екземпляр на HTTP запит (правильно для сервісів з DbContext)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
//builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.MapControllers();
app.Run();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Map controller routes
app.MapControllers();

app.Run();
