namespace MinhaApi.DTOs;

public class ProdutoCreateRequest
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int CategoriaId { get; set; }
}
