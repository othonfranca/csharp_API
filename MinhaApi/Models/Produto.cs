namespace MinhaApi.Models;

public class Produto 
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    // Chave Estrangeira (O ID da Categoria no SQL)
    public int CategoriaId { get; set; }
    // Propriedade de Navegação (Para o C# conseguir ler os dados da categoria)
    public Categoria? Categoria { get; set; }
}