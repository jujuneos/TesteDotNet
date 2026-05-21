using Domain.Entidades;
using Domain.Interfaces;
using FluentResults;
using FluentValidation;
using MediatR;

namespace Application.UseCases.Usuarios.Commands;

public record CriarUsuarioCommand(
    string Nome,
    string Email,
    DateTime DataNascimento
) : IRequest<Result<CriarUsuarioCommandResponse>>
{
    public class Validator : AbstractValidator<CriarUsuarioCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 50 caracteres.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("O email deve ser válido.");
        }
    }
};

public class CriarUsuarioCommandHandler : IRequestHandler<CriarUsuarioCommand, Result<CriarUsuarioCommandResponse>>
{
    private readonly IUsuarioRepository usuarioRepository;

    public CriarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    {
        this.usuarioRepository = usuarioRepository;
    }

    public async Task<Result<CriarUsuarioCommandResponse>> Handle(CriarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuarioExistente = await usuarioRepository.GetByEmailAsync(request.Email);

        if (usuarioExistente is not null)
            return Result.Fail(new Error("Já existe um usuário cadastrado com esse e-mail."));

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            DataNascimento = request.DataNascimento,
            DataCriacao = DateTime.UtcNow
        };

        await usuarioRepository.CreateAsync(usuario);

        return Result.Ok(new CriarUsuarioCommandResponse(usuario.Id));
    }
}

public record CriarUsuarioCommandResponse(int id);

public record CriarEditarUsuarioRequest(string nome, string email, DateTime dataNascimento);
