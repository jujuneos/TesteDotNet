using Domain.Interfaces;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Usuarios.Commands;

public record EditarUsuarioCommand(int Id, string Nome, string Email, DateTime dataNascimento) : IRequest<Result>
{
    public class Validator : AbstractValidator<EditarUsuarioCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("O Id deve ser maior que zero.");
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("O email deve ser válido.");
            RuleFor(x => x.dataNascimento)
                .LessThan(DateTime.UtcNow).WithMessage("A data de nascimento deve ser no passado.");
        }
    }
};

public class EditarUsuarioCommandHandler : IRequestHandler<EditarUsuarioCommand, Result>
{
    private readonly IUsuarioRepository usuarioRepository;

    public EditarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    {
        this.usuarioRepository = usuarioRepository;
    }

    public async Task<Result> Handle(EditarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByIdAsync(request.Id);

        if (usuario is null)
            return Result.Fail(new Error("Usuário não encontrado."));

        var usuarioExistente = await usuarioRepository.GetByEmailAsync(request.Email);

        if (usuarioExistente is not null && usuarioExistente.Id != request.Id)
            return Result.Fail(new Error("Já existe um usuário cadastrado com esse e-mail."));

        usuario.Nome = request.Nome;
        usuario.Email = request.Email;
        usuario.DataNascimento = request.dataNascimento;
        usuario.DataEdicao = DateTime.UtcNow;

        try
        {
            await usuarioRepository.UpdateAsync(usuario);
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(new Error("Ocorreu um erro ao atualizar o usuário: " + ex.Message));
        }

        return Result.Ok();
    }
}
