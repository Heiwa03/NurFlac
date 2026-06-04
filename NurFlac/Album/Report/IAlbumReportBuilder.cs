// ============================================================
// PATTERN: Builder (Creational)
// Role   : Builder interface — declares the steps for building
//          the multi-file album execution report thread-safely.
// ============================================================
namespace NurFlac.Album.Report;

public interface IAlbumReportBuilder
{
    IAlbumReportBuilder AddSuccess(string fileName);
    IAlbumReportBuilder AddFailure(string fileName, string reason);
    AlbumReport         Build();
}
