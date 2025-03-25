using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Core.DTOs;
using Project.Core.Interfaces;
using Project.Core.Models;
using Project.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Service.Contracts
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.CustomerRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<CustomerDto>>(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                throw;
            }
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            try
            {
                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(id);
                return _mapper.Map<CustomerDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer with id {id}");
                throw;
            }
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto)
        {
            try
            {
                var customer = _mapper.Map<Customer>(customerDto);
                await _unitOfWork.CustomerRepository.AddAsync(customer);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<CustomerDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        public async Task UpdateCustomerAsync(int id, CustomerDto customerDto)
        {
            try
            {
                var existingCustomer = await _unitOfWork.CustomerRepository.GetByIdAsync(id);
                if (existingCustomer == null)
                    throw new KeyNotFoundException("Customer not found");

                _mapper.Map(customerDto, existingCustomer);
                existingCustomer.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.CustomerRepository.Update(existingCustomer);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer with id {id}");
                throw;
            }
        }

        public async Task DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(id);
                if (customer == null)
                    throw new KeyNotFoundException("Customer not found");

                _unitOfWork.CustomerRepository.Delete(customer);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer with id {id}");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerDto>> FilterCustomersAsync(string? name, string? region, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var customers = await _unitOfWork.CustomerRepository.GetAllAsync();

                if (!string.IsNullOrEmpty(name))
                {
                    customers = customers.Where(c =>
                        c.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        c.LastName.Contains(name, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(region))
                {
                    customers = customers.Where(c =>
                        c.Region.Equals(region, StringComparison.OrdinalIgnoreCase));
                }

                if (startDate.HasValue)
                {
                    customers = customers.Where(c => c.RegistrationDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    customers = customers.Where(c => c.RegistrationDate <= endDate.Value);
                }

                return _mapper.Map<IEnumerable<CustomerDto>>(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering customers");
                throw;
            }
        }
    }
}
