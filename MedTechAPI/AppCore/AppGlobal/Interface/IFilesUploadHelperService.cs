using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.AppGlobal.Interface
{
    public interface IFilesUploadHelperService
    {
        Task<GenResponse<string>> UploadSingleImageFileToPath(IFormFile file, string fileName="", string path = "", CancellationToken ct = default!);
    }
}
