using FougeraClub1.Data;
using FougeraClub1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FougeraClub1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
       .AddRoles<IdentityRole>()
       .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    string[] roleNames = { "Staff", "Manager" };

                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new IdentityRole(roleName));
                        }
                    }

                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    // Staff account
                    var staffEmail = "staff@fougeraclub.com";
                    if (await userManager.FindByEmailAsync(staffEmail) == null)
                    {
                        var staffUser = new ApplicationUser
                        {
                            UserName = staffEmail,
                            Email = staffEmail,
                            EmailConfirmed = true
                        };
                        await userManager.CreateAsync(staffUser, "Staff@12345");
                        await userManager.AddToRoleAsync(staffUser, "Staff");
                    }

                    // Manager account
                    var managerEmail = "manager@fougeraclub.com";
                    if (await userManager.FindByEmailAsync(managerEmail) == null)
                    {
                        var managerUser = new ApplicationUser
                        {
                            UserName = managerEmail,
                            Email = managerEmail,
                            EmailConfirmed = true
                        };
                        await userManager.CreateAsync(managerUser, "Manager@12345");
                        await userManager.AddToRoleAsync(managerUser, "Manager");
                    }
                
            }

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
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
