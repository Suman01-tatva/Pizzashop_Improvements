namespace PizzaShop.Repository.Interfaces;

public interface IUnitWorkRepository
{
    public Task BeginTransactionAsync();
    public Task CommitTransactionAsync();
    public Task RollbackTransactionAsync();
}
