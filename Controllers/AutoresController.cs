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
public class AutoresController(AppDbContext db) : ControllerBase
{
    /// <summary>Lista todos os autores.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AutorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AutorResponse>>> Listar()
    {
        var autores = await db.Autores.AsNoTracking().OrderBy(a => a.Nome).ToListAsync();
        return Ok(autores.Select(AutorResponse.From));
    }

    /// <summary>Busca um autor pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AutorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutorResponse>> ObterPorId(int id)
    {
        var autor = await db.Autores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (autor is null)
            return NotFound(NaoEncontrado("Autor", id));

        return Ok(AutorResponse.From(autor));
    }

    /// <summary>Lista os livros de um autor.</summary>
    [HttpGet("{id:int}/livros")]
    [ProducesResponseType(typeof(IEnumerable<LivroResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<LivroResponse>>> ListarLivros(int id)
    {
        if (!await db.Autores.AnyAsync(a => a.Id == id))
            return NotFound(NaoEncontrado("Autor", id));

        var livros = await db.Livros
            .AsNoTracking()
            .Include(l => l.Autor)
            .Where(l => l.AutorId == id)
            .OrderBy(l => l.Titulo)
            .ToListAsync();

        return Ok(livros.Select(LivroResponse.From));
    }

    /// <summary>Cria um novo autor.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AutorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AutorResponse>> Criar([FromBody] AutorRequest request)
    {
        var autor = new Autor
        {
            Nome = request.Nome.Trim(),
            Nacionalidade = request.Nacionalidade.Trim()
        };

        db.Autores.Add(autor);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = autor.Id, version = "1.0" },
            AutorResponse.From(autor));
    }

    /// <summary>Atualiza um autor existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AutorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutorResponse>> Atualizar(int id, [FromBody] AutorRequest request)
    {
        var autor = await db.Autores.FindAsync(id);
        if (autor is null)
            return NotFound(NaoEncontrado("Autor", id));

        autor.Nome = request.Nome.Trim();
        autor.Nacionalidade = request.Nacionalidade.Trim();
        await db.SaveChangesAsync();

        return Ok(AutorResponse.From(autor));
    }

    /// <summary>Remove um autor (somente se não tiver livros).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remover(int id)
    {
        var autor = await db.Autores.FindAsync(id);
        if (autor is null)
            return NotFound(NaoEncontrado("Autor", id));

        if (await db.Livros.AnyAsync(l => l.AutorId == id))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Operação não permitida",
                Detail = "O autor possui livros cadastrados. Remova os livros antes de excluir o autor."
            });
        }

        db.Autores.Remove(autor);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static ProblemDetails NaoEncontrado(string recurso, int id) => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Recurso não encontrado",
        Detail = $"{recurso} com id {id} não foi encontrado."
    };
}
