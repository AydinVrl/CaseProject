using Project.Core.DTOs;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text;

namespace Project.Web.Service
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;

            
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]!);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task AddJwtTokenAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding JWT token to request");
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            await AddJwtTokenAsync();

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"GET request failed for {endpoint}");
                throw new ApiException($"API request failed: {ex.Message}", ex.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            await AddJwtTokenAsync();

            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(endpoint, jsonContent);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"POST request failed for {endpoint}");
                throw new ApiException($"API request failed: {ex.Message}", ex.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

        public async Task<bool> PutAsync(string endpoint, object data)
        {
            await AddJwtTokenAsync();

            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync(endpoint, jsonContent);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"PUT request failed for {endpoint}");
                throw new ApiException($"API request failed: {ex.Message}", ex.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            await AddJwtTokenAsync();

            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"DELETE request failed for {endpoint}");
                throw new ApiException($"API request failed: {ex.Message}", ex.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

  
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            return await PostAsync<AuthResponseDto>("api/auth/login", loginDto);
        }

        public async Task<UserDto?> RegisterAsync(RegisterDto registerDto)
        {
            return await PostAsync<UserDto>("api/auth/register", registerDto);
        }

        public async Task<List<CustomerDto>?> GetCustomersAsync()
        {
            return await GetAsync<List<CustomerDto>>("api/customers");
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            return await GetAsync<CustomerDto>($"api/customers/{id}");
        }

        public async Task<CustomerDto?> CreateCustomerAsync(CustomerDto customerDto)
        {
            return await PostAsync<CustomerDto>("api/customers", customerDto);
        }

        public async Task<bool> UpdateCustomerAsync(int id, CustomerDto customerDto)
        {
            return await PutAsync($"api/customers/{id}", customerDto);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            return await DeleteAsync($"api/customers/{id}");
        }
    }

}
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApiException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }

