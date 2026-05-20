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
    private readonly 
}

public record CriarUsuarioCommandResponse(int id);
