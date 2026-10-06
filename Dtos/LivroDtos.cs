using System.ComponentModel.DataAnnotations;
using BibliotecaApi.Models;

namespace BibliotecaApi.Dtos;

public class LivroRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter entre {2} e {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ISBN é obrigatório.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "O ISBN deve ter entre {2} e {1} caracteres.")]
    public string Isbn { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Ano de publicação inválido.")]
    public int AnoPublicacao { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "AutorId inválido.")]
    public int AutorId { get; set; }
}

public record LivroResponse(int Id, string Titulo, string Isbn, int AnoPublicacao, int AutorId, string? AutorNome)
{
    public static LivroResponse From(Livro livro) =>
        new(livro.Id, livro.Titulo, livro.Isbn, livro.AnoPublicacao, livro.AutorId, livro.Autor?.Nome);
}
