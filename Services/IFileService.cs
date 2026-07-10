using Microsoft.AspNetCore.Http;

namespace ReleaseDashboard.Services
{
    public interface IFileService
    {
        Task<List<(string OriginalName,
                   string StoredName,
                   string Path,
                   string ContentType,
                   long Size)>> UploadFilesAsync(
                        List<IFormFile> files,
                        string folderName);
    }
}
