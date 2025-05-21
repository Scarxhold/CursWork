using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CursWork.Models;
using System.Threading.Tasks;
using System.Linq;
using CursWork.Services;
using CursWork.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace CursWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CursWorkContext _context;

        public CustomersController(CursWorkContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            try
            {
                var customers = await _context.Customers.ToListAsync();
                if (customers == null || !customers.Any())
                {
                    return NotFound("No customers found.");
                }
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Code == id);

                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }

                return Ok(new
                {
                    Name = customer.Name,
                    Email = customer.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    return BadRequest("Customer data is null.");
                }
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Code }, customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} ({ex.InnerException?.Message})");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
                {
                    return BadRequest("Email і пароль обов’язкові.");
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => (c.Email == login.Email || c.Name == login.Email) && c.Password == login.Password);

                if (customer == null)
                {
                    return Unauthorized("Невірний email, ім’я або пароль.");
                }

                if (string.IsNullOrEmpty(customer.Role))
                {
                    return BadRequest("Роль користувача не визначена.");
                }

                // Створюємо claims для авторизації
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, customer.Email),
            new Claim("CustomerCode", customer.Code.ToString()), // Додаємо CustomerCode
            new Claim(ClaimTypes.Role, customer.Role) // Додаємо роль
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                };

                // Створюємо cookie автентифікації
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return Ok(new
                {
                    id = customer.Code,
                    code = customer.Code,
                    role = customer.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Помилка сервера: {ex.Message} ({ex.InnerException?.Message})");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            try
            {
                if (id != customer.Code)
                {
                    return BadRequest("Customer ID mismatch.");
                }

                _context.Entry(customer).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(id))
                    {
                        return NotFound($"Customer with ID {id} not found.");
                    }
                    else
                    {
                        return StatusCode(500, "Concurrency error occurred.");
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} ({ex.InnerException?.Message})");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                return Ok($"Customer with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} ({ex.InnerException?.Message})");
            }
        }

        [HttpGet("customers-with-transactions")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersWithTransactions()
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => _context.Transactions.Any(t => t.Customer_Code == c.Code))
                    .ToListAsync();

                if (customers == null || !customers.Any())
                {
                    return NotFound("No customers with transactions found.");
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} ({ex.InnerException?.Message})");
            }
        }

        [HttpGet("transaction-count-per-customer")]
        public async Task<ActionResult> GetTransactionCountPerCustomer()
        {
            try
            {
                var data = await _context.Transactions
                    .GroupBy(t => t.Customer_Code)
                    .Select(g => new
                    {
                        CustomerID = g.Key,
                        TransactionCount = g.Count()
                    })
                    .ToListAsync();

                if (data == null || !data.Any())
                {
                    return NotFound("No transaction data found.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} ({ex.InnerException?.Message})");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<Customer>> Register([FromBody] RegisterDTO register)
        {
            try
            {
                if (register == null || string.IsNullOrEmpty(register.Name) || string.IsNullOrEmpty(register.Phone) || string.IsNullOrEmpty(register.Email) || string.IsNullOrEmpty(register.Password))
                {
                    return BadRequest("Ім’я, телефон, email і пароль обов’язкові.");
                }

                if (await _context.Customers.AnyAsync(c => c.Email == register.Email))
                {
                    return BadRequest("Користувач з таким email вже існує.");
                }

                var customer = new Customer
                {
                    Name = register.Name,
                    Phone = register.Phone, // Добавляем Phone
                    Email = register.Email,
                    Password = register.Password,
                    Role = register.Role ?? "customer",
                    RegistrationDate = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = customer.Code,
                    code = customer.Code,
                    role = customer.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Помилка сервера: {ex.Message} ({ex.InnerException?.Message})");
            }
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Code == id);
        }
    }
}