using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

public class SecurityTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // 1. Definição do Esquema (Bearer)
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Insira o token JWT aqui"
        };

        // 2. Garante que Components existe
        document.Components ??= new OpenApiComponents();

        // 3. CORREÇÃO DO ERRO CS0019:
        // Se for nulo, instanciamos usando a interface correta
        if (document.Components.SecuritySchemes == null)
        {
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
        }
        
        // Agora adicionamos com segurança
        document.Components.SecuritySchemes.Add("Bearer", securityScheme);

        // 4. Criação da Referência e do Requisito (Dicionário)
        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);

        var requirement = new OpenApiSecurityRequirement
        {
            { 
                schemeReference, 
                new List<string>() 
            }
        };

        // 5. Aplica ao documento
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirement);

        return Task.CompletedTask;
    }
}