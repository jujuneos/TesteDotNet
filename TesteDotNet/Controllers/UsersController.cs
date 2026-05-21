using Application.UseCases.Usuarios.Commands;
using Application.UseCases.Usuarios.Queries;
using Domain.Entidades;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TesteDotNet.Controllers;

[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ISender sender;

    public UsersController(ISender sender)
    {
        this.sender = sender;
    }

    /// <summary>
    /// Retorna uma lista paginada de todos os usuários cadastrados.
    /// </summary>
    /// <param name="pagina">Número da página.</param>
    /// <returns>Lista de usuários.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll([FromQuery] int pagina = 1)
    {
        var usuarios = await sender.Send(new ObterTodosOsUsuariosQuery(pagina));
        return Ok(usuarios.Value);
    }

    /// <summary>
    /// Obtém um usuário pelo ID fornecido.
    /// Se o usuário não for encontrado, uma mensagem de erro será retornada.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    /// <returns>O usuário encontrado.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var usuario = await sender.Send(new ObterUsuarioPorIdQuery(id));
        return Ok(usuario.Value);
    }

    /// <summary>
    /// Cria usuário com os dados fornecidos.
    /// O nome deve conter no máximo 50 caracteres e o e-mail deve ser válido.
    /// Se um usuário já existir com o mesmo e-mail, uma mensagem de erro será retornada.
    /// </summary>
    /// <param name="nome">Nome do usuário.</param>
    /// <param name="email">E-mail do usuário.</param>
    /// <param name="dataNascimento">Data de nascimento do usuário.</param>
    /// <returns>O ID do usuário criado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CriarEditarUsuarioRequest request)
    {
        var result = await sender.Send(new CriarUsuarioCommand(request.nome, request.email, request.dataNascimento));
        return Ok(result.Value);
    }

    /// <summary>
    /// Atualiza os dados de um usuário existente com base no ID fornecido.
    /// Caso o usuário não seja encontrado, uma mensagem de erro será retornada.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    /// <param name="nome">Nome do usuário.</param>
    /// <param name="email">E-mail do usuário.</param>
    /// <param name="dataNascimento">Data de nascimento do usuário.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CriarEditarUsuarioRequest request)
    {
        var result = await sender.Send(new EditarUsuarioCommand(id, request.nome, request.email, request.dataNascimento));
        return Ok(result);
    }

    /// <summary>
    /// Deleta um usuário existente com base no ID fornecido.
    /// Caso o usuário não seja encontrado, uma mensagem de erro será retornada.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    /// <returns>O ID do usuário deletado.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await sender.Send(new DeletarUsuarioCommand(id));
        return Ok(result);
    }
};
