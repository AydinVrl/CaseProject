using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Core.DTOs;
using Project.Service.Interfaces;

namespace Project.Web.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public IEnumerable<CustomerDto> Customers { get; set; } = Enumerable.Empty<CustomerDto>();

        public IndexModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task OnGetAsync()
        {
            Customers = await _customerService.GetAllCustomersAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(id);
                TempData["SuccessMessage"] = "Customer deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting customer: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
