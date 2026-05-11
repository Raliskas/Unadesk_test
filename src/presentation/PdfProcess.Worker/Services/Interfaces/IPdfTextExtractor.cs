namespace PdfProcess.Worker.Services.Interfaces;

public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(string filePath);
}
