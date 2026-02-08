using Microsoft.EntityFrameworkCore.Storage;
using TestAPI.Infrastructure.Repositories.SqlServer;

namespace TestAPI.Infrastructure
{
    /// <summary>
    /// Implements the Unit of Work pattern for managing transactions and coordinating repository access.
    /// This implementation is thread-safe through scoped lifetime in DI container.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Semaphore for thread-safe transaction management
        private readonly SemaphoreSlim _transactionLock = new SemaphoreSlim(1, 1);

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Commits all changes to the database in a thread-safe manner.
        /// </summary>
        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                // If there's an active transaction, roll it back
                if (_transaction != null)
                {
                    await RollbackTransactionAsync(cancellationToken);
                }
                throw;
            }
        }

        /// <summary>
        /// Begins a new database transaction in a thread-safe manner.
        /// </summary>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _transactionLock.WaitAsync(cancellationToken);
            try
            {
                if (_transaction != null)
                {
                    throw new InvalidOperationException("A transaction is already in progress.");
                }

                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            }
            catch
            {
                _transactionLock.Release();
                throw;
            }
        }

        /// <summary>
        /// Commits the current transaction in a thread-safe manner.
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No transaction is in progress.");
                }

                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
                _transactionLock.Release();
            }
        }

        /// <summary>
        /// Rolls back the current transaction in a thread-safe manner.
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
                
                // Only release if we own the lock
                if (_transactionLock.CurrentCount == 0)
                {
                    _transactionLock.Release();
                }
            }
        }

        /// <summary>
        /// Disposes the Unit of Work and releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose pattern implementation.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _transaction?.Dispose();
                    _transactionLock?.Dispose();
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
