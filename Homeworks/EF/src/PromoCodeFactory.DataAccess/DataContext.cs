using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Data;

namespace PromoCodeFactory.DataAccess
{
    public class DataContext : DbContext
    {
        /// <summary>
        /// Промокоды
        /// </summary>
        public DbSet<PromoCode> PromoCodes { get; set; }

        /// <summary>
        /// Клиенты
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Группы предпочтений
        /// </summary>
        public DbSet<Preference> Preferences { get; set; }

        /// <summary>
        /// Роли работников
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Работники
        /// </summary>
        public DbSet<Employee> Employees { get; set; }

        public DataContext()
        {

        }

        public DataContext(DbContextOptions<DataContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region В рамках пункта 4 ДЗ: Прописываем связи
            //// Эту связь прописывать не обязательно, EF её строит самостоятельно (по соглашению об обнаружении связей)
            //modelBuilder.Entity<Employee>().
            //    HasOne(r => r.Role);
            //// Эту связь прописывать не обязательно, EF её строит самостоятельно (по соглашению об обнаружении связей)
            //modelBuilder.Entity<PromoCode>().
            //    HasOne(pc => pc.Preference);
            
            // Эту связь пришлось прописывать явно. EF хоть и строит её самостоятельно, но каскадное удаление промокодов не обеспечивает.
            modelBuilder.Entity<Customer>().
                HasMany(c => c.PromoCodes).
                WithOne(q => q.Owner).//WithOne(q => q.CustomerInfo).
                OnDelete(DeleteBehavior.Cascade);

            #region Многие-ко-многим через промежуточную сущность
            // Прописываем составной PK
            modelBuilder.Entity<CustomerPreference>()
                .HasKey(customerPreference => new { customerPreference.CustomerId, customerPreference.PreferenceId });
            // Один-ко-многим к сущности "Клиент"
            modelBuilder.Entity<CustomerPreference>()
                .HasOne(customerPreference => customerPreference.Customer)
                .WithMany(customer => customer.Preferences)
                .HasForeignKey(customerPreference => customerPreference.CustomerId);
            // Один-ко-многим к сущности "Предпочтения"
            modelBuilder.Entity<CustomerPreference>()
                .HasOne(customerPreference => customerPreference.Preference)
                .WithMany() // т.к. в Preference нет ссылок на Customer, то в скобках пусто
                .HasForeignKey(cp => cp.PreferenceId);
            #endregion

            #endregion


            // Можно так...
            modelBuilder.Entity<PromoCode>(pc =>
            {
                pc.Property(c => c.Code).HasMaxLength(20);
                pc.Property(c => c.ServiceInfo).HasMaxLength(200);
                pc.Property(c => c.PartnerName).HasMaxLength(100);
            });

            // ..., а можно так
            modelBuilder.Entity<Customer>().Property(c => c.FirstName).HasMaxLength(200);
            modelBuilder.Entity<Customer>().Property(c => c.LastName).HasMaxLength(200);
            modelBuilder.Entity<Customer>().Property(c => c.Email).HasMaxLength(200);

            modelBuilder.Entity<Preference>().Property(c => c.Name).HasMaxLength(200);

            modelBuilder.Entity<Role>().Property(c => c.Name).HasMaxLength(100);
            modelBuilder.Entity<Role>().Property(c => c.Description).HasMaxLength(500);

            modelBuilder.Entity<Employee>(e =>
            {
                e.Property(c => c.FirstName).HasMaxLength(200);
                e.Property(c => c.LastName).HasMaxLength(200);
                e.Property(c => c.Email).HasMaxLength(200);
            });
        }
    }
}
