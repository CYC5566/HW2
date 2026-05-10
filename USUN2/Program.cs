using Microsoft.AspNetCore.Authentication.Cookies;
using USUN2.Business;
using USUN2.Data.Infrastructure;

namespace USUN2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.SlidingExpiration = true;
                });
            builder.Services.AddFinancePreferenceStack();

            var app = builder.Build();

            if (app.Configuration.GetValue("Database:AutoInitialize", false))
            {
                var logger = app.Services.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("FinanceDatabase");
                var cs = app.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("缺少連線字串 DefaultConnection。");
                var dbDir = Path.Combine(app.Environment.ContentRootPath, "DB");
                await FinanceDatabaseBootstrap.EnsureReadyAsync(cs, dbDir, logger);
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
        }
    }
}
