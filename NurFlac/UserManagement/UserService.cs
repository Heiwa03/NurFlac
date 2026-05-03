using NurFlac.UserManagement.Entities;

namespace NurFlac.UserManagement;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<User> GetOrCreateUserAsync(long telegramId) 
        => _repository.GetOrCreateByTelegramIdAsync(telegramId);

    public async Task ApplyStrikeAsync(long telegramId, string reason)
    {
        // This is a simplified method for IUserService, 
        // normally handled by the Moderation Mediator.
        var user = await _repository.GetOrCreateByTelegramIdAsync(telegramId);
        user.StrikeCount++;
        await _repository.UpdateUserAsync(user);
    }

    public Task UpdateUserAsync(User user) => _repository.UpdateUserAsync(user);
}
