namespace NurFlac.Album.Report;

public sealed class AlbumReport
{
    public long                         TelegramId   { get; init; }
    public DateTime                     GeneratedAt  { get; init; }
    public IReadOnlyList<string>        Accepted     { get; init; } = [];
    public IReadOnlyList<(string File, string Reason)> Rejected { get; init; } = [];

    public string ToMarkdown()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"**Album Upload Report** — {GeneratedAt:u}");
        sb.AppendLine($"✅ Accepted: {Accepted.Count}   ❌ Rejected: {Rejected.Count}");
        sb.AppendLine();

        if (Accepted.Count > 0)
        {
            sb.AppendLine("**Passed:**");
            foreach (var f in Accepted) sb.AppendLine($"  • {f}");
        }

        if (Rejected.Count > 0)
        {
            sb.AppendLine("**Failed:**");
            foreach (var (file, reason) in Rejected)
                sb.AppendLine($"  ✗ `{file}` — {reason}");
        }

        return sb.ToString().TrimEnd();
    }
}
