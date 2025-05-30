using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IWaitingTokenRepository
{
    public Task<List<WaitingToken>> GetAllWaitingTokens();
    public Task<List<WaitingToken>> GetWaitingTokensBySectionId(int sectionId);

    public Task<WaitingToken> GetWaitingTokensById(int id);

    public Task<bool> UpdateWaitingToken(WaitingToken token);

    public Task<bool> CreateWaitingToken(WaitingToken token);
    public Task DeleteWaitingToken(int id, int userId);

}
