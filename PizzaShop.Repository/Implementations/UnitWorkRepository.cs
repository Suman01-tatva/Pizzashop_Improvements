namespace PizzaShop.Repository.Implementations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

public class UnitWorkRepository: IUnitWorkRepository
{
  private readonly PizzashopContext _context;
  private IDbContextTransaction _transaction;

  public UnitWorkRepository(PizzashopContext context)
  {
    _context = context;
  }

  public async Task BeginTransactionAsync()
  {
    _transaction = await _context.Database.BeginTransactionAsync();
  }

  public async Task CommitTransactionAsync()
  {
    await _transaction?.CommitAsync()!;
  }

  public async Task RollbackTransactionAsync()
  {
    await _transaction?.RollbackAsync()!;
  }

}