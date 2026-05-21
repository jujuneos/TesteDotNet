using Application.UseCases.Usuarios.Commands;
using Domain.Entidades;
using Domain.Interfaces;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Application.Tests.UseCases.Usuarios.Commands;

public class CriarUsuarioCommandTests
{
    private readonly CriarUsuarioCommand.Validator validator = new();

    [Fact]
    public void Deve_Retornar_Erro_Quando_Nome_For_Vazio()
    {
        // Arrange
        var command = new CriarUsuarioCommand(
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
    public void Deve_Retornar_Erro_Quando_Nome_Exceder_50_Caracteres()
    {
        // Arrange
        var nome = new string('A', 51);

        var command = new CriarUsuarioCommand(
            nome,
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage("O nome deve ter no máximo 50 caracteres.");
    }

    [Fact]
    public void Deve_Retornar_Erro_Quando_Email_For_Vazio()
    {
        // Arrange
        var command = new CriarUsuarioCommand(
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
        var command = new CriarUsuarioCommand(
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
        var command = new CriarUsuarioCommand(
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddDays(1));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataNascimento)
            .WithErrorMessage("A data de nascimento deve ser no passado.");
    }

    [Fact]
    public void Deve_Passar_Quando_Command_For_Valido()
    {
        // Arrange
        var command = new CriarUsuarioCommand(
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CriarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository usuarioRepository;
    private readonly CriarUsuarioCommandHandler handler;

    public CriarUsuarioCommandHandlerTests()
    {
        usuarioRepository = Substitute.For<IUsuarioRepository>();
        handler = new CriarUsuarioCommandHandler(usuarioRepository);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Email_Ja_Existir()
    {
        // Arrange
        var command = new CriarUsuarioCommand(
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns(new Usuario());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message == "Já existe um usuário cadastrado com esse e-mail.");

        await usuarioRepository
            .DidNotReceive()
            .CreateAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Deve_Criar_Usuario_Quando_Email_Nao_Existir()
    {
        // Arrange
        var command = new CriarUsuarioCommand(
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        usuarioRepository
            .When(x => x.CreateAsync(Arg.Any<Usuario>()))
            .Do(call =>
            {
                var usuario = call.Arg<Usuario>();
                usuario.Id = 1;
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.id);

        await usuarioRepository
            .Received(1)
            .CreateAsync(Arg.Is<Usuario>(x =>
                x.Nome == command.Nome &&
                x.Email == command.Email &&
                x.DataNascimento == command.DataNascimento));
    }

    [Fact]
    public async Task Deve_Definir_DataCriacao_Ao_Criar_Usuario()
    {
        // Arrange
        var command = new CriarUsuarioCommand(
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

        Usuario? usuarioCriado = null;

        usuarioRepository
            .GetByEmailAsync(command.Email)
            .Returns((Usuario?)null);

        usuarioRepository
            .When(x => x.CreateAsync(Arg.Any<Usuario>()))
            .Do(call =>
            {
                usuarioCriado = call.Arg<Usuario>();
            });

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(usuarioCriado);
        Assert.True(usuarioCriado!.DataCriacao <= DateTime.UtcNow);
        Assert.True(usuarioCriado.DataCriacao >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Deve_Chamar_GetByEmailAsync_Com_Email_Correto()
    {
        // Arrange
        var command = new CriarUsuarioCommand(
            "Usuário Teste",
            "teste@email.com",
            DateTime.UtcNow.AddYears(-20));

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
}