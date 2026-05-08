using Microsoft.EntityFrameworkCore;
using PdfProcess.Data.Models;
using PdfProcess.Worker.DataAccess.Mapping;

namespace PdfProcess.Worker.DataAccess;

public class DocumentContext : DbContext
{
    public DocumentContext(DbContextOptions<DocumentContext> options)
    : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new DocumentMap())

        ;
        base.OnModelCreating(modelBuilder);
    }
}
