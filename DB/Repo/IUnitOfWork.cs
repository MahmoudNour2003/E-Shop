using System;
using System.Threading.Tasks;

namespace DB.Repo
{
    public interface IUnitOfWork : IDisposable
    {
        IEntityRepo<APP_USER> Users { get; }
        IEntityRepo<Address> Addresses { get; }
        IEntityRepo<Category> Categories { get; }
        IEntityRepo<Order> Orders { get; }
        IEntityRepo<Product> Products { get; }
        IEntityRepo<Order_Item> OrderItems { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
