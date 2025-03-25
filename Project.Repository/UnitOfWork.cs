using Project.Core.Interfaces;
using Project.Core.Models;
using Project.Repository.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IRepository<User>? _userRepository;
        private IRepository<Customer>? _customerRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        public IRepository<User> UserRepository => _userRepository ??= new GenericRepository<User>(_context);

        public IRepository<Customer> CustomerRepository => _customerRepository ??= new GenericRepository<Customer>(_context);

        public async Task<bool> CommitAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
