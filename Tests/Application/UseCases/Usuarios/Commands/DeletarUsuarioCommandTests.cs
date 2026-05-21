using Application.UseCases.Usuarios.Commands;
using Domain.Entidades;
using Domain.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Application.Tests.UseCases.Usuarios.Commands;

public class DeletarUsuarioCommandTests
{
    private readonly IUsuarioRepository usuarioRepository;
    private readonly DeletarUsuarioCommandHandler handler;

    public DeletarUsuarioCommandTests()
    {
        usuarioRepository = Substitute.For<IUsuarioRepository>();
        handler = new DeletarUsuarioCommandHandler(usuarioRepository);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Usuario_Nao_For_Encontrado()
    {
        // Arrange
        var command = new DeletarUsuarioCommand(1);

        usuarioRepository
            .GetByIdAsync(command.id)
            .Returns((Usuario?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message == "Usuário não encontrado.");

        await usuarioRepository
            .DidNotReceive()
            .DeleteUserAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Deve_Deletar_Usuario_Quando_Usuario_Existir()
    {
        // Arrange
        var command = new DeletarUsuarioCommand(1);

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Usuário Teste",
            Email = "teste@email.com"
        };

        usuarioRepository
            .GetByIdAsync(command.id)
            .Returns(usuario);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);
        Assert.Equal(command.id, result.Value.id);

        await usuarioRepository
            .Received(1)
            .DeleteUserAsync(usuario);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Ocorrer_DbUpdateException_Ao_Deletar()
    {
        // Arrange
        var command = new DeletarUsuarioCommand(1);

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Usuário Teste",
            Email = "teste@email.com"
        };

        usuarioRepository
            .GetByIdAsync(command.id)
            .Returns(usuario);

        usuarioRepository
            .DeleteUserAsync(usuario)
            .Returns<Task>(_ =>
            {
                throw new DbUpdateException("Erro no banco de dados.");
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message.Contains("Ocorreu um erro ao deletar o usuário:"));

        Assert.Contains(result.Errors,
            x => x.Message.Contains("Erro no banco de dados."));
    }

    [Fact]
    public async Task Deve_Chamar_GetByIdAsync_Com_Id_Correto()
    {
        // Arrange
        var command = new DeletarUsuarioCommand(10);

        usuarioRepository
            .GetByIdAsync(command.id)
            .Returns(new Usuario());

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await usuarioRepository
            .Received(1)
            .GetByIdAsync(command.id);
    }

    [Fact]
    public async Task Deve_Chamar_DeleteUserAsync_Com_Usuario_Correto()
    {
        // Arrange
        var command = new DeletarUsuarioCommand(5);

        var usuario = new Usuario
        {
            Id = 5,
            Nome = "Usuário Teste"
        };

        usuarioRepository
            .GetByIdAsync(command.id)
            .Returns(usuario);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await usuarioRepository
            .Received(1)
            .DeleteUserAsync(Arg.Is<Usuario>(x =>
                x.Id == usuario.Id &&
                x.Nome == usuario.Nome));
    }

    [Fact]
    public async Task Nao_Deve_Retornar_Falha_Quando_Delete_For_Executado_Com_Sucesso()
    {
        // Arrange
        var command = new DeletarUsuarioCommand(1);

        var usuario = new Usuario
        {
            Id = 1
        };

        usuarioRepository
            .GetByIdAsync(command.id)
            .Returns(usuario);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailed);
        Assert.True(result.IsSuccess);
    }
}