namespace NurFlac.Audio.Models;

public sealed class AudioFormatRegistry
{
    private static readonly AudioFormat[] _formats =
    [
        new() { Extension = ".flac", MimeType = "audio/flac",    DisplayName = "FLAC",  IsLossless = true  },
        new() { Extension = ".flac", MimeType = "audio/x-flac",  DisplayName = "FLAC",  IsLossless = true  },
        new() { Extension = ".wav",  MimeType = "audio/wav",     DisplayName = "WAV",   IsLossless = true  },
        new() { Extension = ".wav",  MimeType = "audio/x-wav",   DisplayName = "WAV",   IsLossless = true  },
        new() { Extension = ".wav",  MimeType = "audio/wave",    DisplayName = "WAV",   IsLossless = true  },
        new() { Extension = ".m4a",  MimeType = "audio/x-m4a",   DisplayName = "ALAC",  IsLossless = true  },
        new() { Extension = ".m4a",  MimeType = "audio/mp4",     DisplayName = "ALAC",  IsLossless = true  },
        new() { Extension = ".aiff", MimeType = "audio/aiff",    DisplayName = "AIFF",  IsLossless = true  },
        new() { Extension = ".aiff", MimeType = "audio/x-aiff",  DisplayName = "AIFF",  IsLossless = true  },
        new() { Extension = ".aif",  MimeType = "audio/aiff",    DisplayName = "AIFF",  IsLossless = true  },
        new() { Extension = ".mp3",  MimeType = "audio/mpeg",    DisplayName = "MP3",   IsLossless = false },
        new() { Extension = ".mp3",  MimeType = "audio/mp3",     DisplayName = "MP3",   IsLossless = false },
        new() { Extension = ".aac",  MimeType = "audio/aac",     DisplayName = "AAC",   IsLossless = false },
        new() { Extension = ".ogg",  MimeType = "audio/ogg",     DisplayName = "OGG",   IsLossless = false },
        new() { Extension = ".opus", MimeType = "audio/opus",    DisplayName = "Opus",  IsLossless = false },
    ];

    public AudioFormat? GetByExtension(string extension)
    {
        var ext = extension.ToLowerInvariant();
        return Array.Find(_formats, f => f.Extension == ext);
    }

    public AudioFormat? GetByMimeType(string mimeType)
    {
        var mime = mimeType.ToLowerInvariant();
        return Array.Find(_formats, f => f.MimeType == mime);
    }

    public IEnumerable<AudioFormat> GetLosslessFormats() =>
        _formats.Where(f => f.IsLossless).DistinctBy(f => f.Extension);
}
