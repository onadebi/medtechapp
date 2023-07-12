
namespace MedTechAPI.AppCore.AppGlobal.Interface
{
    interface IRepository<TEntity> where TEntity : class
    {
        int Add(TEntity model);
        int AddRange(IEnumerable<TEntity> entities);
        IQueryable<TEntity> GetAll();
        TEntity GetById(object key);
        int Update(TEntity id);
        int Remove(object key);
    }
}
