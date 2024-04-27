using DataAccess;

using DataAccess.Repository;
using DataAccess.Repository.IRepository;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

});
//builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.MapRazorPages();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{Area=Customer}/{controller=Home}/{action=Index}/{id?}");



//using var scope = app.Services.CreateScope();
//var services = scope.ServiceProvider;
//var context = services.GetRequiredService<ApplicationDbContext>();

//// var identityContext = services.GetRequiredService<AppIdentityDbContext>();
//// var userManager = services.GetRequiredService<UserManager<AppUser>>();

//var logger = services.GetRequiredService<ILogger<Program>>();
//try
//{
//    await context.Database.MigrateAsync();
//    //await identityContext.Database.MigrateAsync();
//   // await AppIdentityDbContextSeed.SeedUsersAsync(userManager);
//}
//catch (Exception ex)
//{
//    logger.LogError(ex,"An error accured during migration");
//}
app.Run();
