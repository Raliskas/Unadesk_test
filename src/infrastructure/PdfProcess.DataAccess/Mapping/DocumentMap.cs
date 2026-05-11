using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PdfProcess.Data.Models;

namespace PdfProcess.DataAccess.Mapping;

public class DocumentMap : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FileName)
            .IsRequired();

        builder.Property(p => p.FilePath)
            .IsRequired();
    }
}
