using HRMS.Application.Interfaces.Repositories;
using HRMS.Domain.Common;
using HRMS.Infrastructure.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private Hashtable? _repositories;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            _repositories ??= new Hashtable();

            var typeName = typeof(T).Name;

            if (!_repositories.ContainsKey(typeName))
            {
                var repositoryType = typeof(GenericRepository<>).MakeGenericType(typeof(T));
                var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
                _repositories.Add(typeName, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[typeName]!;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
