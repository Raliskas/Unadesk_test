using System.ComponentModel.DataAnnotations;

namespace PdfProcess.Data.Configs;

public class StorageConfig
{
    [Required]
    public string PdfStoragePath { get; set; }
}
