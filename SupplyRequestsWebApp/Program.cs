using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SupplyManagement.Models;
using SupplyManagement.Models.Enums;
using SupplyManagement.Services;
using SupplyManagement.WebApp.Data;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() // Log to console
            .WriteTo.File($"C:\\Users\\{Environment.UserName}\\Desktop\\log.txt", rollingInterval: RollingInterval.Day) // Log to file on desktop
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        builder.Services.AddSignalR();

		// Add services to the container.
		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost",
                builder =>
                {
                    builder.WithOrigins("http://localhost:44306"
                        ,"http://localhost:14384"
                        ,"http://localhost:5290"
                        ,"https://localhost:7014") // Replace "port" with the port number your application is running on
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
        });

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
            options.AddPolicy("User", policy => policy.RequireRole("User"));
            options.AddPolicy("Courier", policy => policy.RequireRole("Courier"));
        });
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
        });
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowLocalhost");
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{area=Login}/{controller=Home}/{action=Index}/{id?}"
            );

            endpoints.MapHub<NotificationHub>("/notificationHub");
		});

        // Create Roles
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = ["Admin", "Manager", "Courier", "User"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // Add Admin
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var adminSection = app.Configuration.GetSection("SuperAdmin");
            string email = adminSection.GetSection("Email").Value;
            string password = adminSection.GetSection("Password").Value;
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new User();
                user.Email = email;
                user.UserName = adminSection.GetSection("UserName").Value;
                user.Name = adminSection.GetSection("Name").Value;
                user.Surname = adminSection.GetSection("Surname").Value;
                user.AccountStatus = AccountStatuses.Approved;

                string[] roles = ["Admin", "Manager", "Courier", "User"];

                await userManager.CreateAsync(user, password);
                await userManager.AddToRolesAsync(user, roles);
            }
        }

        app.Run();
    }
}