using Application.UseCases.Usuarios.Commands;
using Domain.Entidades;
using Domain.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Application.Tests.UseCases.Usuarios.Commands;

public class EditarUsuarioCommandValidatorTests
{
    private readonly EditarUsuarioCommand.Validator validator = new();

    [Fact]
    public void Deve_Retornar_Erro_Quando_Id_For_Menor_Ou_Igual_A_Zero()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            0,
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O Id deve ser maior que zero.");
    }

    [Fact]
    public void Deve_Retornar_Erro_Quando_Nome_For_Vazio()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage("O nome é obrigatório.");
    }

    [Fact]
    public void Deve_Retornar_Erro_Quando_Nome_Exceder_100_Caracteres()
    {
        // Arrange
        var nome = new string('A', 101);

        var command = new EditarUsuarioCommand(
            1,
            nome,
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage("O nome deve ter no máximo 100 caracteres.");
    }

    [Fact]
    public void Deve_Retornar_Erro_Quando_Email_For_Vazio()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário Teste",
            "",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("O email é obrigatório.");
    }

    [Fact]
    public void Deve_Retornar_Erro_Quando_Email_For_Invalido()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário Teste",
            "email-invalido",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("O email deve ser válido.");
    }

    [Fact]
    public void Deve_Retornar_Erro_Quando_DataNascimento_For_Futura()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddDays(1));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.dataNascimento)
            .WithErrorMessage("A data de nascimento deve ser no passado.");
    }

    [Fact]
    public void Deve_Passar_Quando_Command_For_Valido()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class EditarUsuarioCommandTests
{
    private readonly IUsuarioRepository usuarioRepository;
    private readonly EditarUsuarioCommandHandler handler;

    public EditarUsuarioCommandTests()
    {
        usuarioRepository = Substitute.For<IUsuarioRepository>();
        handler = new EditarUsuarioCommandHandler(usuarioRepository);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Usuario_Nao_For_Encontrado()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns((Usuario?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message == "Usuário não encontrado.");

        await usuarioRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Email_Pertencer_A_Outro_Usuario()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário Editado",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Usuário Original",
            Email = "original@email.com"
        };

        var usuarioExistente = new Usuario
        {
            Id = 2,
            Email = "teste@email.com"
        };

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(usuario);

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns(usuarioExistente);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message == "Já existe um usuário cadastrado com esse e-mail.");

        await usuarioRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Deve_Permitir_Editar_Quando_Email_For_Do_Proprio_Usuario()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário Editado",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Usuário Original",
            Email = "teste@email.com"
        };

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(usuario);

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns(usuario);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await usuarioRepository
            .Received(1)
            .UpdateAsync(usuario);
    }

    [Fact]
    public async Task Deve_Atualizar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Novo Nome",
            "novo@email.com",
            DateTime.UtcNow.AddYears(-30));

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Nome Antigo",
            Email = "antigo@email.com",
            DataNascimento = DateTime.UtcNow.AddYears(-20)
        };

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(usuario);

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(command.Nome, usuario.Nome);
        Assert.Equal(command.Email, usuario.Email);
        Assert.Equal(command.dataNascimento, usuario.DataNascimento);

        Assert.NotNull(usuario.DataEdicao);

        await usuarioRepository
            .Received(1)
            .UpdateAsync(Arg.Is<Usuario>(x =>
                x.Id == command.Id &&
                x.Nome == command.Nome &&
                x.Email == command.Email &&
                x.DataNascimento == command.dataNascimento));
    }

    [Fact]
    public async Task Deve_Definir_DataEdicao_Ao_Atualizar_Usuario()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Novo Nome",
            "novo@email.com",
            DateTime.UtcNow.AddYears(-25));

        var usuario = new Usuario
        {
            Id = 1
        };

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(usuario);

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(usuario.DataEdicao);

        Assert.True(usuario.DataEdicao <= DateTime.UtcNow);
        Assert.True(usuario.DataEdicao >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Ocorrer_DbUpdateException()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Novo Nome",
            "novo@email.com",
            DateTime.UtcNow.AddYears(-25));

        var usuario = new Usuario
        {
            Id = 1
        };

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(usuario);

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        usuarioRepository
            .UpdateAsync(usuario)
            .Returns<Task>(_ =>
            {
                throw new DbUpdateException("Erro no banco.");
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message.Contains("Ocorreu um erro ao atualizar o usuário:"));

        Assert.Contains(result.Errors,
            x => x.Message.Contains("Erro no banco."));
    }

    [Fact]
    public async Task Deve_Chamar_GetByIdAsync_Com_Id_Correto()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            10,
            "Usuário",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(new Usuario { Id = 10 });

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await usuarioRepository
            .Received(1)
            .GetByIdAsync(command.Id);
    }

    [Fact]
    public async Task Deve_Chamar_GetByEmailAsync_Com_Email_Correto()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Usuário",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(new Usuario { Id = 1 });

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await usuarioRepository
            .Received(1)
            .GetByEmailAsync(command.Email);
    }

    [Fact]
    public async Task Nao_Deve_Retornar_Falha_Quando_Update_For_Executado_Com_Sucesso()
    {
        // Arrange
        var command = new EditarUsuarioCommand(
            1,
            "Novo Nome",
            "novo@email.com",
            DateTime.UtcNow.AddYears(-20));

        usuarioRepository
            .GetByIdAsync(command.Id)
            .Returns(new Usuario { Id = 1 });

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailed);
        Assert.True(result.IsSuccess);
    }
}