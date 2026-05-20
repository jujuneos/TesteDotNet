using Domain.Interfaces;
using FluentResults;
using MediatR;

namespace Application.UseCases.Usuarios.Queries;

public record ObterUsuarioPorIdQuery(int id) : IRequest<Result<UsuarioQueryResponse>>;

public class ObterUsuarioPorIdQueryHandler : IRequestHandler<ObterUsuarioPorIdQuery, Result<UsuarioQueryResponse>>
{
    private readonly IUsuarioRepository usuarioRepository;

    public ObterUsuarioPorIdQueryHandler(IUsuarioRepository usuarioRepository)
    {
        this.usuarioRepository = usuarioRepository;
    }

    public async Task<Result<UsuarioQueryResponse>> Handle(ObterUsuarioPorIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByIdAsync(request.id);

        if (usuario is null)
            return Result.Fail("Usuário não encontrado.");

        var response = new UsuarioQueryResponse
        {
            Nome = usuario.Nome,
            Email = usuario.Email,
            DataNascimento = usuario.DataNascimento
        };

        return Result.Ok(response);
    }
}

public record class UsuarioQueryResponse
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DataNascimento { get; set; }
}

