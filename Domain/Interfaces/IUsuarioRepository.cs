using Domain.Entidades;

namespace Domain.Interfaces;

public interface IUsuarioRepository
{
    public Task<List<Usuario>> GetAllAsync(int pagina);
    public Task<Usuario?> GetByIdAsync(int id);
    public Task<Usuario?> GetByEmailAsync(string email);
    public Task CreateAsync(Usuario usuario);
    public void Update(Usuario usuario);
    public void DeleteUser(Usuario usuario);
}
