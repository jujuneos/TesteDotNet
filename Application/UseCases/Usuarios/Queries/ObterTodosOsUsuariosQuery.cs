using Domain.Entidades;
using Domain.Interfaces;
using FluentResults;
using MediatR;

namespace Application.UseCases.Usuarios.Queries;

public record ObterTodosOsUsuariosQuery(int pagina) : IRequest<Result<IEnumerable<ObterUsuariosResponse>>>;

public class ObterTodosOsUsuariosQueryHandler : IRequestHandler<ObterTodosOsUsuariosQuery, Result<IEnumerable<ObterUsuariosResponse>>>
{
    private readonly IUsuarioRepository usuarioRepository;

    public ObterTodosOsUsuariosQueryHandler(IUsuarioRepository usuarioRepository)
    {
        this.usuarioRepository = usuarioRepository;
    }

    public async Task<Result<IEnumerable<ObterUsuariosResponse>>> Handle(ObterTodosOsUsuariosQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await usuarioRepository.GetAllAsync(request.pagina);

        if (usuarios.Count == 0)
            return Result.Fail("Nenhum usuário encontrado.");

        return Result.Ok(usuarios.AsEnumerable());
    }
}