namespace NurFlac.DuplicateChecking.Models;

public sealed record DuplicateCheckResult(bool IsDuplicate, AudioFingerprint Fingerprint);
