using Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.Usuarios.Commands;

public record DeletarUsuarioCommand(int id) : IRequest<Result<DeletarUsuarioCommandResponse>>;

public class DeletarUsuarioCommandHandler : IRequestHandler<DeletarUsuarioCommand, Result<DeletarUsuarioCommandResponse>>
{
    private readonly IUsuarioRepository usuarioRepository;

    public DeletarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    {
        this.usuarioRepository = usuarioRepository;
    }

    public async Task<Result<DeletarUsuarioCommandResponse>> Handle(DeletarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByIdAsync(request.id);

        if (usuario is null)
            return Result.Fail(new Error("Usuário não encontrado."));

        try
        {
            await usuarioRepository.DeleteUserAsync(usuario);
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(new Error($"Ocorreu um erro ao deletar o usuário: {ex.Message}"));
        }

        return Result.Ok(new DeletarUsuarioCommandResponse(request.id));
    }
}

public record DeletarUsuarioCommandResponse(int id);