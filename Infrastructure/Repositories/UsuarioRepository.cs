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

    public async Task<List<ObterUsuariosResponse>> GetAllAsync(int pagina)
    {
        var usuarios = await ctx
            .Usuario
            .Select(u => new ObterUsuariosResponse
            (
                u.Id,
                u.Nome,
                u.Email,
                u.DataNascimento.ToString("dd/MM/yyyy")
            ))
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
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Usuario usuario)
    {
        ctx.Usuario.Remove(usuario);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        ctx.Usuario.Update(usuario);
        await ctx.SaveChangesAsync();
    }
}