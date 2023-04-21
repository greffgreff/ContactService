using IdentityService.Contracts;
using IdentityService.Exceptions;
using IdentityService.Repository;

namespace IdentityService.Services.Keys;

public class KeyService
{
    private readonly KeyRepository repository;
    private readonly UserService service;
    
    public KeyService(KeyRepository repository, UserService service)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<KeyBundle> GetKeyBundle(string from, string to)
    {
        if (from == to)
        {
            throw new BadKeyBundleRequest();
        }
        return await repository.GetKeyBundleAndDisposeFromUser(to);
    }

    public async Task RegisterExchangeKeys(ExchangeKeys keys)
    {
        var _ = await service.GetUserById(keys.UserId) ?? throw new UserNotFound();
        await repository.CreateOrUpdateKeys(keys.UserId, keys.IdentityKey, keys.SignedPreKey, keys.Signature, keys.OneTimePreKeys);
    }
}