using Telegram.Bot.Types;
using NurFlac.Storage;
using User = NurFlac.UserManagement.Entities.User;
using NurFlac.AudioProcessing.Interfaces;

namespace NurFlac.Handlers;

public class SingleAudioUploadCommand : ICommand
{
    private readonly IAudioProcessor _audioProcessor;
    private readonly IStorageService _storageService;

    public SingleAudioUploadCommand(IAudioProcessor audioProcessor, IStorageService storageService)
    {
        _audioProcessor = audioProcessor;
        _storageService = storageService;
    }

    public async Task ExecuteAsync(Message message, User user)
    {
        throw new NotImplementedException();
    }
}
