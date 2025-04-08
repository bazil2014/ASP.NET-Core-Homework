using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PromoCodeFactory.DataAccess;
using PromoCodeFactory.DataAccess.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;


namespace PromoCodeFactory.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // В рамках работы с миграцией попробовал и иной способ очистки/заполнения БД, ориентируясь на проект выданый лектором.
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                //db.Database.EnsureDeletedAsync();
                db.Database.Migrate();
                Seed(scope.ServiceProvider);
            }


            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        public static void Seed(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();

                if (context.Employees.Count() == 0)
                {
                    context.AddRange(FakeDataFactory.Employees);
                    context.SaveChanges();
                }

                if (context.Preferences.Count() == 0)
                {
                    context.AddRange(FakeDataFactory.Preferences);
                    context.SaveChanges();
                }

                if (context.Customers.Count() == 0)
                {
                    context.AddRange(FakeDataFactory.Customers);
                    context.SaveChanges();
                }
            }
        }

    }
}