using MedTechAPI.AppCore.AppGlobal.Interface;
using MedTechAPI.AppCore.MedicCenter.Repository;
using OnaxTools.Dto.Http;

namespace MedTechAPI.AppCore.AppGlobal.Repository
{
    public class FilesUploadHelperService: IFilesUploadHelperService
    {
        private readonly ILogger<FilesUploadHelperService> _logger;

        public FilesUploadHelperService(ILogger<FilesUploadHelperService> logger)
        {
            _logger = logger;
        }

        public async Task<GenResponse<string>> UploadSingleImageFileToPath(IFormFile file, string fileName ="", string path = "", CancellationToken ct = default!)
        {
            GenResponse<string> objResp = new() { IsSuccess = false };
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
            }
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo info = Directory.CreateDirectory(path);
                }
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"{Guid.NewGuid()}_{file.FileName}";
                }
                path = Path.Join(path, fileName);
                using Stream fileStream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(fileStream, ct);
                objResp.IsSuccess = true;
                objResp.Message = objResp.Result = path;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return GenResponse<string>.Failed($"ERROR: {ex.Message}");
            }
            return objResp;
        }
    }
}
