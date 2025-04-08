using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Data
{
    public class DbInitializer
    {
        private readonly DataContext _dataContext;

        public DbInitializer(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void InitializeDb()
        {
            _dataContext.Database.EnsureDeleted();
            _dataContext.Database.EnsureCreated();

            // Роли создаются когда добавляем работников
            //_dataContext.AddRange(FakeDataFactory.Roles);
            //_dataContext.SaveChanges();

            _dataContext.AddRange(FakeDataFactory.Employees);
            _dataContext.SaveChanges();

            _dataContext.AddRange(FakeDataFactory.Preferences);
            _dataContext.SaveChanges();

            _dataContext.AddRange(FakeDataFactory.Customers);
            _dataContext.SaveChanges();
        }

    }
}
