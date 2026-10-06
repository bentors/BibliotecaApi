using System.ComponentModel.DataAnnotations;
using BibliotecaApi.Models;

namespace BibliotecaApi.Dtos;

public class AutorRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nacionalidade é obrigatória.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "A nacionalidade deve ter entre {2} e {1} caracteres.")]
    public string Nacionalidade { get; set; } = string.Empty;
}

public record AutorResponse(int Id, string Nome, string Nacionalidade)
{
    public static AutorResponse From(Autor autor) =>
        new(autor.Id, autor.Nome, autor.Nacionalidade);
}
