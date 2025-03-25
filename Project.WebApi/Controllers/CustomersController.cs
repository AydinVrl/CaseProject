using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTOs;
using Project.Service.Interfaces;

namespace Project.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return NotFound();

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerDto customerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdCustomer = await _customerService.CreateCustomerAsync(customerDto);
                return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDto customerDto)
        {
            try
            {
                if (id != customerDto.Id)
                    return BadRequest("ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _customerService.UpdateCustomerAsync(id, customerDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Customer with id {id} not found");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Customer with id {id} not found");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> Filter(
            [FromQuery] string? name,
            [FromQuery] string? region,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var customers = await _customerService.FilterCustomersAsync(name, region, startDate, endDate);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering customers");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
