using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Castle.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.DataAccess;
using PromoCodeFactory.DataAccess.Data;
using PromoCodeFactory.DataAccess.Repositories;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;


namespace PromoCodeFactory.WebHost
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            // Не совсем ясно, что даёт добавление сервиса как пары "интерфейс-реализация" ...
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            // ... и что теряется, если так не делать. TODO: надо будет глянуть...
            services.AddScoped(typeof(DbInitializer));


            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite("Filename=../PromoCodeFactory.DataAccess/Promocode.db");
                // Необходимо, что бы создавались свойства-сущности. При этом свойства обязательно должны быть virtual, а сущности иметь заявленный PK.
                options.UseLazyLoadingProxies();
            });
            services.AddOpenApiDocument(options =>
            {
                options.Title = "PromoCode Factory API Doc";
                options.Version = "1.0";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseOpenApi();
            app.UseSwaggerUi(x =>
            {
                x.DocExpansion = "list";
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Пункт 3 ДЗ: Чистим и заполняем заново БД.
            // Отключено в рамках выполнения пункта 8 ДЗ.
            //dbInitializer.InitializeDb();
        }
    }
}