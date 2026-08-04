namespace AgileFlow.Infrastructure.Persistence.Seed;

public class SeedOptions
{
    public const string SectionName = "Seed";

    public List<SeedUserOptions> Users { get; set; } = new();
}

public class SeedUserOptions
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
