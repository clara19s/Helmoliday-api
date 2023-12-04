namespace HELMoliday.Services.ImageUpload;
public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, string fileName);

    Task<Stream> GetFileAsync(string filePath);

    Task RemoveUploadedFileAsync(string fileName);
}