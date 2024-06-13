
using TemplateMVC.Core.Interface;
using TemplateMVC.Core.Services;
using TemplateMVC.Data;
using TemplateMVC.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<ApplicationDbContext>();

builder.Services.AddScoped<IUnitOfWork>(provider =>
        UnitOfWorkFactory.CreateUnitOfWork(context: provider.GetService<ApplicationDbContext>() ?? throw new("ApplicationDbContext")
        ));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    await app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
