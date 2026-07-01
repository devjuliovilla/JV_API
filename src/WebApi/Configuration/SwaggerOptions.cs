namespace WebApi.Configuration;

public class SwaggerOptions
{
    public const string Section = "Swagger";

    public bool AllowSwaggerUi { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public SwaggerContact? Contact { get; set; }
    public SwaggerLicense? License { get; set; }
}

public class SwaggerContact
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class SwaggerLicense
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
