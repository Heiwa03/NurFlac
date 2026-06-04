// ============================================================
// PATTERN: Strategy (Behavioral)
// Role   : ConcreteStrategy — interchangeable hashing algorithms
//          used by LedgerService before writing DB entries.
// ============================================================
namespace NurFlac.Ledger.Hashing;

public interface IHashStrategy
{
    string AlgorithmName { get; }
    Task<string> ComputeAsync(Stream data, CancellationToken ct = default);
}
