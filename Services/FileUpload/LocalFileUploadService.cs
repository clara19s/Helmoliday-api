using HELMoliday.Services.ImageUpload;

namespace HELMoliday.Services.FileUpload;
public class LocalFileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        var fileExtension = Path.GetExtension(file.FileName);
        var fileNameAndExtension = $"{fileName}{fileExtension}";
        var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileNameAndExtension);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/{fileNameAndExtension}";
    }

    public async Task RemoveUploadedFileAsync(string fileName)
    {
        var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    public Task<Stream> GetFileAsync(string filePath)
    {
        var fileNameAndExtension = Path.GetFileName(filePath);
        var file = File.OpenRead(Path.Combine(_environment.WebRootPath, "uploads", fileNameAndExtension));
        return Task.FromResult<Stream>(file);
    }
}
