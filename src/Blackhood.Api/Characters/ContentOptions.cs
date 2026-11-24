namespace Blackhood.Api.Characters;

public sealed class ContentOptions
{
    /// <summary>Folder with character JSON files, relative to the API content root.</summary>
    public string CharactersPath { get; set; } = "../../content/characters";
    /// <summary>Folder with portrait images, relative to the API content root.</summary>
    public string PortraitsPath { get; set; } = "../../content/portraits";
    /// <summary>Import missing characters from <see cref="CharactersPath"/> on startup.</summary>
    public bool ImportOnStartup { get; set; } = true;
    /// <summary>Host code used for single-player tables until more hosts unlock.</summary>
    public string DefaultHost { get; set; } = "H01";
}

public sealed class ContentPaths(IWebHostEnvironment env, Microsoft.Extensions.Options.IOptions<ContentOptions> options)
{
    public string Characters { get; } = Path.GetFullPath(Path.Combine(env.ContentRootPath, options.Value.CharactersPath));
    public string Portraits { get; } = Path.GetFullPath(Path.Combine(env.ContentRootPath, options.Value.PortraitsPath));
}
