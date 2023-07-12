using MedTechAPI.AppCore.AppGlobal.Interface;
using MedTechAPI.Persistence;
using Microsoft.EntityFrameworkCore;
//using System.Data.Entity;

namespace MedTechAPI.AppCore.AppGlobal.Repository
{
    public class GenericRepository<T>: IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private DbSet<T> _entities;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public virtual int Add(T model)
        {
            throw new NotImplementedException();
        }

        public virtual int AddRange(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<T> GetAll()
        {
            var resp = this._Entities;
            return resp;
        }

        public virtual T GetById(object key)
        {
            return this._Entities.Find(key);
        }

        public virtual int Remove(object key)
        {
            throw new NotImplementedException();
        }

        public int Update(T id)
        {
            throw new NotImplementedException();
        }

        #region HELPERS
        private DbSet<T> _Entities
        {
            get
            {
                if(_entities == null)
                {
                    _entities = _context.Set<T>();
                }
                return _entities;
            }
        }
        #endregion
    }
}
