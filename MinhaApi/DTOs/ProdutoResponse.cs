namespace MinhaApi.DTOs;

public class ProdutoResponse
{
    public int Id {get;set;}
    public string Nome {get;set;} = string.Empty;
    public decimal Preco {get;set;}
    public string CategoriaNome {get;set;} = string.Empty;
}