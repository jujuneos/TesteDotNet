using Application.UseCases.Usuarios.Queries;
using Domain.Interfaces;
using NSubstitute;

namespace Application.Tests.UseCases.Usuarios.Queries;

public class ObterTodosOsUsuariosQueryTests
{
    private readonly IUsuarioRepository usuarioRepository;
    private readonly ObterTodosOsUsuariosQueryHandler handler;

    public ObterTodosOsUsuariosQueryTests()
    {
        usuarioRepository = Substitute.For<IUsuarioRepository>();
        handler = new ObterTodosOsUsuariosQueryHandler(usuarioRepository);
    }

    [Fact]
    public async Task Deve_Retornar_Falha_Quando_Nenhum_Usuario_For_Encontrado()
    {
        // Arrange
        var query = new ObterTodosOsUsuariosQuery(1);

        usuarioRepository
            .GetAllAsync(query.pagina)
            .Returns(new List<ObterUsuariosResponse>());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        Assert.Contains(result.Errors,
            x => x.Message == "Nenhum usuário encontrado.");
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Usuarios_Forem_Encontrados()
    {
        // Arrange
        var query = new ObterTodosOsUsuariosQuery(1);

        var usuarios = new List<ObterUsuariosResponse>
        {
            new(
                1,
                "Usuário 1",
                "usuario1@email.com",
                "01/01/2000"
            ),
            new(
                2,
                "Usuário 2",
                "usuario2@email.com",
                "15/05/1995"
            )
        };

        usuarioRepository
            .GetAllAsync(query.pagina)
            .Returns(usuarios);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        var resultado = result.Value.ToList();

        Assert.Equal(2, resultado.Count);

        Assert.Equal(1, resultado[0].Id);
        Assert.Equal("Usuário 1", resultado[0].Nome);
        Assert.Equal("01/01/2000", resultado[0].DataNascimento);

        Assert.Equal(2, resultado[1].Id);
        Assert.Equal("Usuário 2", resultado[1].Nome);
        Assert.Equal("15/05/1995", resultado[1].DataNascimento);
    }

    [Fact]
    public async Task Deve_Chamar_GetAllAsync_Com_Pagina_Correta()
    {
        // Arrange
        var query = new ObterTodosOsUsuariosQuery(5);

        usuarioRepository
            .GetAllAsync(query.pagina)
            .Returns(new List<ObterUsuariosResponse>
            {
                new(
                    1,
                    "Usuário",
                    "usuario@email.com",
                    "01/01/2000"
                )
            });

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        await usuarioRepository
            .Received(1)
            .GetAllAsync(query.pagina);
    }

    [Fact]
    public async Task Deve_Retornar_Usuarios_Como_Enumerable()
    {
        // Arrange
        var query = new ObterTodosOsUsuariosQuery(1);

        var usuarios = new List<ObterUsuariosResponse>
        {
            new(
                1,
                "Usuário Teste",
                "teste@email.com",
                "01/01/2000"
            )
        };

        usuarioRepository
            .GetAllAsync(query.pagina)
            .Returns(usuarios);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Value is IEnumerable<ObterUsuariosResponse>);
    }

    [Fact]
    public async Task Nao_Deve_Retornar_Falha_Quando_Existirem_Usuarios()
    {
        // Arrange
        var query = new ObterTodosOsUsuariosQuery(1);

        usuarioRepository
            .GetAllAsync(query.pagina)
            .Returns(new List<ObterUsuariosResponse>
            {
                new(
                    1,
                    "Usuário",
                    "usuario@email.com",
                    "01/01/2000"
                )
            });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailed);
        Assert.True(result.IsSuccess);
    }
}