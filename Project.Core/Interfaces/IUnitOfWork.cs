using Project.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> UserRepository { get; }
        IRepository<Customer> CustomerRepository { get; }
        Task<bool> CommitAsync();
    }
}
