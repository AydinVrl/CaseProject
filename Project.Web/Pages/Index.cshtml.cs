using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Core.DTOs;
using Project.Service.Interfaces;

namespace Project.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IAuthService _authService;

    [BindProperty]
    public LoginDto LoginDto { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IndexModel(IAuthService authService)
    {
        _authService = authService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var response = await _authService.LoginAsync(LoginDto);

            HttpContext.Session.SetString("JwtToken", response.Token);

            return RedirectToPage("/Customers/Index");
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
