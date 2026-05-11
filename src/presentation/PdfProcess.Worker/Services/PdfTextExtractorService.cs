using PdfProcess.Worker.Services.Interfaces;
using System.Text;
using UglyToad.PdfPig;

namespace PdfProcess.Worker.Services;

public class PdfTextExtractorService : IPdfTextExtractor
{
    public async Task<string> ExtractTextAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var textBuilder = new StringBuilder();

            try
            {
                using (var pdf = PdfDocument.Open(filePath))
                {
                    foreach (var page in pdf.GetPages())
                    {
                        var pageText = page.Text;
                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            textBuilder.AppendLine(pageText);
                        }
                    }
                }

                return textBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract text from PDF: {ex.Message}", ex);
            }
        });
    }
}
