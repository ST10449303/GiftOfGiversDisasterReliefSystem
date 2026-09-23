using GiftOfGiversDisasterReliefSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// ==========================================
// ADD MVC SERVICES
// ==========================================

builder.Services.AddControllersWithViews();


// ==========================================
// REGISTER HTTP CLIENT
// Used to communicate with Azure Functions
// ==========================================

builder.Services.AddHttpClient();


// ==========================================
// CONNECT TO AZURE SQL DATABASE
// ==========================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


// ==========================================
// ASP.NET CORE IDENTITY
// ==========================================

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Users do not need to confirm their email
    // before they can log in.
    options.SignIn.RequireConfirmedAccount = false;

    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();


// ==========================================
// BUILD APPLICATION
// ==========================================

var app = builder.Build();


// ==========================================
// CREATE DEFAULT ROLES
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();


    // ==========================================
    // CREATE DONOR ROLE
    // ==========================================

    if (!await roleManager.RoleExistsAsync("Donor"))
    {
        var donorRole = await roleManager.CreateAsync(
            new IdentityRole("Donor"));

        if (!donorRole.Succeeded)
        {
            throw new Exception(
                "Unable to create the Donor role.");
        }
    }


    // ==========================================
    // CREATE EMPLOYEE ROLE
    // ==========================================

    if (!await roleManager.RoleExistsAsync("Employee"))
    {
        var employeeRole = await roleManager.CreateAsync(
            new IdentityRole("Employee"));

        if (!employeeRole.Succeeded)
        {
            throw new Exception(
                "Unable to create the Employee role.");
        }
    }
}


// ==========================================
// CONFIGURE HTTP REQUEST PIPELINE
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// ==========================================
// HTTPS
// ==========================================

app.UseHttpsRedirection();


// ==========================================
// STATIC FILES
// ==========================================

app.UseStaticFiles();


// ==========================================
// ROUTING
// ==========================================

app.UseRouting();


// ==========================================
// AUTHENTICATION & AUTHORIZATION
// ==========================================

app.UseAuthentication();

app.UseAuthorization();


// ==========================================
// MVC ROUTES
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);


// ==========================================
// ASP.NET IDENTITY RAZOR PAGES
// ==========================================

app.MapRazorPages();


// ==========================================
// RUN APPLICATION
// ==========================================

app.Run();