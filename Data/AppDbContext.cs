using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Livro> Livros => Set<Livro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Autor>(e =>
        {
            e.ToTable("Autores");
            e.HasKey(a => a.Id);
            e.Property(a => a.Nome).IsRequired().HasMaxLength(150);
            e.Property(a => a.Nacionalidade).IsRequired().HasMaxLength(80);
        });

        modelBuilder.Entity<Livro>(e =>
        {
            e.ToTable("Livros");
            e.HasKey(l => l.Id);
            e.Property(l => l.Titulo).IsRequired().HasMaxLength(200);
            e.Property(l => l.Isbn).IsRequired().HasMaxLength(20);
            e.HasIndex(l => l.Isbn).IsUnique();

            e.HasOne(l => l.Autor)
                .WithMany(a => a.Livros)
                .HasForeignKey(l => l.AutorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
