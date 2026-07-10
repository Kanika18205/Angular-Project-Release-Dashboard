using Microsoft.AspNetCore.Http;

namespace ReleaseDashboard.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB
        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<(string OriginalName,
                                string StoredName,
                                string Path,
                                string ContentType,
                                long Size)>> UploadFilesAsync(
                                    List<IFormFile> files,
                                    string folderName)
        {
            var uploadedFiles = new List<(string, string, string, string, long)>();

            if (files == null || files.Count == 0)
                return uploadedFiles;

            var uploadPath = Path.Combine(
                 _environment.WebRootPath,
                 "Uploads",
                 folderName);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                if (file.Length > MaxFileSize)
                    throw new Exception( $"{file.FileName} exceeds the 100 MB limit.");

                var storedName = $"{Guid.NewGuid()}_{file.FileName}";

                var fullPath = Path.Combine(uploadPath, storedName);
                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                uploadedFiles.Add((
                    file.FileName,
                    storedName,
                    $"/Uploads/{folderName}/{storedName}",
                    file.ContentType,
                    file.Length
                ));
            }
            return uploadedFiles;
        }
    }
}