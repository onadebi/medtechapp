using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.AppGlobal.Interface
{
    public interface ICommonBaseRepo<TCreateEntity, TUpdateEntity, TFetchEntity> 
        where TCreateEntity : class 
        where TUpdateEntity : class
        where TFetchEntity : class
    {
        Task<GenResponse<int>> Add(TCreateEntity model);
        Task<GenResponse<bool>> Update(TUpdateEntity model);
        Task<GenResponse<List<TFetchEntity>>> FetchAll(bool includeInActive = false);
        Task<GenResponse<TFetchEntity>> FetchById(object id);
        Task<GenResponse<bool>> Remove(object Id);
    }
}
