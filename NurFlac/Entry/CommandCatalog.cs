namespace NurFlac.Entry;

public sealed class CommandCatalog : ICommandCatalog
{
    private readonly Dictionary<string, CommandRegistration> _index;
    private readonly IReadOnlyCollection<CommandRegistration> _all;

    public CommandCatalog(IEnumerable<CommandRegistration> registrations)
    {
        var list = registrations.ToList();
        _all = list.AsReadOnly();
        _index = new Dictionary<string, CommandRegistration>(StringComparer.OrdinalIgnoreCase);

        foreach (var registration in list)
        {
            _index[Normalize(registration.Name)] = registration;

            foreach (var alias in registration.Aliases)
            {
                _index[Normalize(alias)] = registration;
            }
        }
    }

    public bool TryResolve(string rawCommandKey, out CommandRegistration registration)
        => _index.TryGetValue(Normalize(rawCommandKey), out registration!);

    public IReadOnlyCollection<CommandRegistration> GetAll() => _all;

    private static string Normalize(string key) => key.Trim().TrimStart('/').ToLowerInvariant();
}
