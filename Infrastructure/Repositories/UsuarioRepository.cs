using Domain.Entidades;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    public AppDbContext ctx { get; set; }

    public UsuarioRepository(AppDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<List<Usuario>> GetAllAsync(int pagina)
    {
        var usuarios = await ctx
            .Usuario
            .Select(u => new Usuario
            {
                Nome = u.Nome,
                Email = u.Email,
                DataNascimento = u.DataNascimento
            })
            .AsNoTracking()
            .ToListAsync();

        var totalRegistros = usuarios.Count;

        return usuarios.Skip((pagina - 1) * totalRegistros).Take(totalRegistros).ToList();
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await ctx.Usuario.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await ctx.Usuario.FirstOrDefaultAsync(u => u.Email.Equals(email));
    }

    public async Task CreateAsync(Usuario usuario)
    {
        await ctx.Usuario.AddAsync(usuario);
    }

    public void DeleteUser(Usuario usuario)
    {
        ctx.Usuario.Remove(usuario);
    }

    public void Update(Usuario usuario)
    {
        ctx.Usuario.Update(usuario);
    }
}
