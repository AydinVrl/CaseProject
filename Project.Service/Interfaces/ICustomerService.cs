using Project.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Service.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto);
        Task UpdateCustomerAsync(int id, CustomerDto customerDto);
        Task DeleteCustomerAsync(int id);
        Task<IEnumerable<CustomerDto>> FilterCustomersAsync(string? name, string? region, DateTime? startDate, DateTime? endDate);
    }
}
