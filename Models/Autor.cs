namespace BibliotecaApi.Models;

public class Autor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Nacionalidade { get; set; } = string.Empty;

    public ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
