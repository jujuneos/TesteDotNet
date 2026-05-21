using Application.UseCases.Usuarios.Queries;
using Domain.Entidades;
using Domain.Interfaces;
using NSubstitute;

namespace Application.Tests.UseCases.Usuarios.Queries;

public class ObterUsuarioPorIdQueryTests
{
    private readonly IUsuarioRepository usuarioRepository;
    private readonly ObterUsuarioPorIdQueryHandler handler;

    public ObterUsuarioPorIdQueryTests()
    {
        usuarioRepository = Substitute.For<IUsuarioRepository>();
        handler = new ObterUsuarioPorIdQueryHandler(usuarioRepository);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Usuario_Nao_For_Encontrado()
    {
        // Arrange
        var query = new ObterUsuarioPorIdQuery(1);

        usuarioRepository
            .GetByIdAsync(query.id)
            .Returns((Usuario?)null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message == "Usuário não encontrado.");
    }

    [Fact]
    public async Task Deve_Retornar_Usuario_Quando_Usuario_For_Encontrado()
    {
        // Arrange
        var dataNascimento = new DateTime(2000, 1, 1);

        var query = new ObterUsuarioPorIdQuery(1);

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Usuário Teste",
            Email = "teste@email.com",
            DataNascimento = dataNascimento
        };

        usuarioRepository
            .GetByIdAsync(query.id)
            .Returns(usuario);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        Assert.Equal(usuario.Nome, result.Value.Nome);
        Assert.Equal(usuario.Email, result.Value.Email);
        Assert.Equal(usuario.DataNascimento, result.Value.DataNascimento);
    }

    [Fact]
    public async Task Deve_Mapear_Corretamente_O_Response()
    {
        // Arrange
        var query = new ObterUsuarioPorIdQuery(10);

        var usuario = new Usuario
        {
            Id = 10,
            Nome = "João Silva",
            Email = "joao@email.com",
            DataNascimento = new DateTime(1995, 5, 15)
        };

        usuarioRepository
            .GetByIdAsync(query.id)
            .Returns(usuario);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var response = result.Value;

        Assert.Equal("João Silva", response.Nome);
        Assert.Equal("joao@email.com", response.Email);
        Assert.Equal(new DateTime(1995, 5, 15), response.DataNascimento);
    }

    [Fact]
    public async Task Deve_Chamar_GetByIdAsync_Com_Id_Correto()
    {
        // Arrange
        var query = new ObterUsuarioPorIdQuery(5);

        usuarioRepository
            .GetByIdAsync(query.id)
            .Returns(new Usuario
            {
                Id = 5,
                Nome = "Usuário",
                Email = "usuario@email.com"
            });

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        await usuarioRepository
            .Received(1)
            .GetByIdAsync(query.id);
    }

    [Fact]
    public async Task Nao_Deve_Retornar_Falha_Quando_Usuario_For_Encontrado()
    {
        // Arrange
        var query = new ObterUsuarioPorIdQuery(1);

        usuarioRepository
            .GetByIdAsync(query.id)
            .Returns(new Usuario
            {
                Id = 1,
                Nome = "Usuário Teste",
                Email = "teste@email.com",
                DataNascimento = new DateTime(2000, 1, 1)
            });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Deve_Retornar_Instancia_De_UsuarioQueryResponse()
    {
        // Arrange
        var query = new ObterUsuarioPorIdQuery(1);

        usuarioRepository
            .GetByIdAsync(query.id)
            .Returns(new Usuario
            {
                Id = 1,
                Nome = "Usuário Teste",
                Email = "teste@email.com",
                DataNascimento = new DateTime(2000, 1, 1)
            });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsType<UsuarioQueryResponse>(result.Value);
    }
}