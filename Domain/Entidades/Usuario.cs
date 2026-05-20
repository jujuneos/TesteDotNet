namespace Domain.Entidades;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DataNascimento { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataEdicao { get; set; }
}
