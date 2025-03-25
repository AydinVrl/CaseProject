using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Core.DTOs;
using Project.Service.Interfaces;

namespace Project.Web.Pages.Customers
{
    public class CreateModel : PageModel
    {
        private readonly ICustomerService _customerService;

        [BindProperty]
        public CustomerDto CustomerDto { get; set; } = new();

        public CreateModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public IActionResult OnGet()
        {
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
                await _customerService.CreateCustomerAsync(CustomerDto);
                TempData["SuccessMessage"] = "Customer created successfully";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating customer: {ex.Message}");
                return Page();
            }
        }
    }
}
