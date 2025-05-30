using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class WaitingTokenRepository : IWaitingTokenRepository
{
    private readonly PizzashopContext _context;

    public WaitingTokenRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<List<WaitingToken>> GetAllWaitingTokens()
    {
        var waitingTokens = await _context.WaitingTokens.Where(t => t.IsDeleted == false && t.IsAssigned == true)
                                                        .Include(c => c.Customer)
                                                        .ToListAsync();
        return waitingTokens;
    }

    public async Task<List<WaitingToken>> GetWaitingTokensBySectionId(int sectionId)
    {
        var waitingTokens = await _context.WaitingTokens.Where(t => t.IsDeleted == false && (t.SectionId == sectionId || sectionId == 0) && t.IsAssigned == false)
                                                        .Include(c => c.Customer)
                                                        .ToListAsync();
        return waitingTokens;
    }

    public async Task<WaitingToken> GetWaitingTokensById(int id)
    {
        var waitingTokens = await _context.WaitingTokens.Where(t => t.IsDeleted == false && t.Id == id)
        .Include(w => w.Customer)
        .FirstOrDefaultAsync();
        return waitingTokens!;
    }

    public async Task<bool> UpdateWaitingToken(WaitingToken token)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            var existToken = await connection.QuerySingleAsync<WaitingToken>("SELECT * FROM GetWaitingTokenById(@Id)", new { Id = token.Id });
            if (existToken == null)
                return false;
            existToken.CustomerId = token.CustomerId;
            existToken.NoOfPeople = token.NoOfPeople;
            existToken.SectionId = token.SectionId;
            existToken.IsAssigned = token.IsAssigned;
            existToken.ModifiedBy = token.ModifiedBy;
            existToken.ModifiedAt = DateTime.UtcNow;

            await _context.Database.ExecuteSqlRawAsync("CALL update_waiting_token({0},{1},{2},{3},{4},{5})", existToken.Id, existToken.CustomerId, existToken.SectionId, existToken.IsAssigned!, existToken.NoOfPeople!, existToken.ModifiedBy!);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> CreateWaitingToken(WaitingToken token)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "CALL create_new_waiting_token({0}::integer,{1}::integer,{2}::integer,{3}::integer)",
                token.CustomerId,
                token.SectionId,
                token.NoOfPeople.HasValue ? token.NoOfPeople.Value : (object)DBNull.Value,
                token.CreatedBy.HasValue ? token.CreatedBy.Value : (object)DBNull.Value
            );
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task DeleteWaitingToken(int id, int userId)
    {
        await _context.Database.ExecuteSqlRawAsync("CALL delete_waiting_token({0},{1})", id, userId);
    }

}