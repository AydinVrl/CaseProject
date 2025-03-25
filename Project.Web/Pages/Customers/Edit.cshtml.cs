using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Core.DTOs;
using Project.Service.Interfaces;

namespace Project.Web.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly ICustomerService _customerService;

        [BindProperty]
        public CustomerDto CustomerDto { get; set; } = new();

        public EditModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            CustomerDto = customer;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _customerService.UpdateCustomerAsync(CustomerDto.Id, CustomerDto);
                TempData["SuccessMessage"] = "Customer updated successfully";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating customer: {ex.Message}");
                return Page();
            }
        }
    }
}
