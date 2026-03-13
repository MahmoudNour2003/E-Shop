using DB.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace DB.Repo
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DBContext _context;
        private IDbContextTransaction _transaction;
        private IEntityRepo<APP_USER> _users;
        private IEntityRepo<Address> _addresses;
        private IEntityRepo<Category> _categories;
        private IEntityRepo<Order> _orders;
        private IEntityRepo<Product> _products;
        private IEntityRepo<Order_Item> _orderItems;

        public UnitOfWork(DBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEntityRepo<APP_USER> Users
        {
            get { return _users ??= new EntityRepo<APP_USER>(_context); }
        }

        public IEntityRepo<Address> Addresses
        {
            get { return _addresses ??= new EntityRepo<Address>(_context); }
        }

        public IEntityRepo<Category> Categories
        {
            get { return _categories ??= new EntityRepo<Category>(_context); }
        }

        public IEntityRepo<Order> Orders
        {
            get { return _orders ??= new EntityRepo<Order>(_context); }
        }

        public IEntityRepo<Product> Products
        {
            get { return _products ??= new EntityRepo<Product>(_context); }
        }

        public IEntityRepo<Order_Item> OrderItems
        {
            get { return _orderItems ??= new EntityRepo<Order_Item>(_context); }
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
