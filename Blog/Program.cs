using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Blog.Data;
using Blog.Models;
using Blog.Utilites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using System;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {

             
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>options.UseSqlServer(connectionString));
           
            builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddMvc(option => option.EnableEndpointRouting = false).AddNewtonsoftJson();
            builder.Services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/Login";
            });
            builder.Services.AddScoped<IDbInitializer,DbInitializer>();
            builder.Services.AddNotyf(config => { config.DurationInSeconds = 10; config.IsDismissable = true; config.Position = NotyfPosition.BottomRight; });
            var app = builder.Build();
            DataSeeding();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseNotyf(); 
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "subAreaRoute",
                    template: "{area:exists}/{subarea:exists}/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });





            app.Run();

            void DataSeeding()
            {
                using(var scope = app.Services.CreateScope()) 
                {
                    var DbInitilaize = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                    DbInitilaize.Initialize();
                }
            }
        }
    }
}