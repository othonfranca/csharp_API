namespace MinhaApi.DTOs;

public class ProdutoGetRequest
{
    public string? Nome { get; set; }
    public int? CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }

    // Valores padrão para paginação
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = 10;
    
}