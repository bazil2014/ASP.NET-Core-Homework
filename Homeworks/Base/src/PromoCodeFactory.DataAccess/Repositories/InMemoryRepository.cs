using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
namespace PromoCodeFactory.DataAccess.Repositories
{
    public class InMemoryRepository<T>: IRepository<T> where T: BaseEntity
    {
        protected IEnumerable<T> Data { get; set; }

        public InMemoryRepository(IEnumerable<T> data)
        {
            Data = data;
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data);
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
        }

        public Task<Guid?> AddAsync(T entity)
        {
            return Task.Run(() =>
            {
                if (!(Data is IList<T> list))
                    return null;
                list.Add(entity);
                return (Guid?)entity.Id;
            });
        }

        public Task<bool> UpdateAsync(T entity)
        {
            return Task.Run(() =>
            {
                T innerEntity = Data.FirstOrDefault(x => x.Id == entity.Id);
                if (!(Data is IList<T> list))
                    return false;
                int idx = list.IndexOf(innerEntity);
                if (idx < 0)
                    return false;
                list[idx] = entity;
                return true;
            });
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            return Task.Run(() =>
            {
                if (!(Data is IList<T> list))
                    return false;
                T entity = Data.FirstOrDefault(x => x.Id == id);
                if (entity == null)
                    return false;
                list.Remove(Data.FirstOrDefault(x => x.Id == id));
                return true;
            });
        }
    }
}