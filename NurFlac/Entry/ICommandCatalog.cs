namespace NurFlac.Entry;

public interface ICommandCatalog
{
    bool TryResolve(string rawCommandKey, out CommandRegistration registration);
    IReadOnlyCollection<CommandRegistration> GetAll();
}
