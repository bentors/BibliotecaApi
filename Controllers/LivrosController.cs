using Asp.Versioning;
using BibliotecaApi.Data;
using BibliotecaApi.Dtos;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class LivrosController(AppDbContext db) : ControllerBase
{
    /// <summary>Lista livros, com filtros opcionais por autor e título.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LivroResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LivroResponse>>> Listar(
        [FromQuery] int? autorId,
        [FromQuery] string? titulo)
    {
        IQueryable<Livro> query = db.Livros.AsNoTracking().Include(l => l.Autor);

        if (autorId.HasValue)
            query = query.Where(l => l.AutorId == autorId.Value);

        if (!string.IsNullOrWhiteSpace(titulo))
            query = query.Where(l => EF.Functions.Like(l.Titulo, $"%{titulo}%"));

        var livros = await query.OrderBy(l => l.Titulo).ToListAsync();
        return Ok(livros.Select(LivroResponse.From));
    }

    /// <summary>Busca um livro pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LivroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LivroResponse>> ObterPorId(int id)
    {
        var livro = await db.Livros
            .AsNoTracking()
            .Include(l => l.Autor)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (livro is null)
            return NotFound(NaoEncontrado("Livro", id));

        return Ok(LivroResponse.From(livro));
    }

    /// <summary>Cadastra um novo livro.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LivroResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LivroResponse>> Criar([FromBody] LivroRequest request)
    {
        var autor = await db.Autores.FindAsync(request.AutorId);
        if (autor is null)
            return AutorInvalido(request.AutorId);

        var isbn = request.Isbn.Trim();
        if (await db.Livros.AnyAsync(l => l.Isbn == isbn))
            return IsbnDuplicado(isbn);

        var livro = new Livro
        {
            Titulo = request.Titulo.Trim(),
            Isbn = isbn,
            AnoPublicacao = request.AnoPublicacao,
            AutorId = request.AutorId,
            Autor = autor
        };

        db.Livros.Add(livro);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = livro.Id, version = "1.0" },
            LivroResponse.From(livro));
    }

    /// <summary>Atualiza um livro existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(LivroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LivroResponse>> Atualizar(int id, [FromBody] LivroRequest request)
    {
        var livro = await db.Livros.FindAsync(id);
        if (livro is null)
            return NotFound(NaoEncontrado("Livro", id));

        var autor = await db.Autores.FindAsync(request.AutorId);
        if (autor is null)
            return AutorInvalido(request.AutorId);

        var isbn = request.Isbn.Trim();
        if (await db.Livros.AnyAsync(l => l.Isbn == isbn && l.Id != id))
            return IsbnDuplicado(isbn);

        livro.Titulo = request.Titulo.Trim();
        livro.Isbn = isbn;
        livro.AnoPublicacao = request.AnoPublicacao;
        livro.AutorId = request.AutorId;
        livro.Autor = autor;

        await db.SaveChangesAsync();

        return Ok(LivroResponse.From(livro));
    }

    /// <summary>Remove um livro.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        var livro = await db.Livros.FindAsync(id);
        if (livro is null)
            return NotFound(NaoEncontrado("Livro", id));

        db.Livros.Remove(livro);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static ProblemDetails NaoEncontrado(string recurso, int id) => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Recurso não encontrado",
        Detail = $"{recurso} com id {id} não foi encontrado."
    };

    private BadRequestObjectResult AutorInvalido(int autorId) => BadRequest(new ProblemDetails
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Requisição inválida",
        Detail = $"O autor com id {autorId} não existe."
    });

    private ConflictObjectResult IsbnDuplicado(string isbn) => Conflict(new ProblemDetails
    {
        Status = StatusCodes.Status409Conflict,
        Title = "ISBN duplicado",
        Detail = $"Já existe um livro cadastrado com o ISBN {isbn}."
    });
}
