using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CursWork.Models;
using CursWork.Services;
using CursWork.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CursWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly CursWorkContext _context;
        private readonly TransactionService _transactionService;

        public TransactionsController(CursWorkContext context, TransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            return await _context.Transactions
                                 .Include(t => t.Customer)
                                 .Include(t => t.Employee)
                                 .ToListAsync();
        }

        // GET: api/Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions
                                           .Include(t => t.Customer)
                                           .Include(t => t.Employee)
                                           .FirstOrDefaultAsync(t => t.Code == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<Transaction>> PostTransaction([FromBody] CreateTransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Дані транзакції відсутні.");
            }

            // Оголосимо customer поза блоком if
            Customer customer = null;
            int customerCode;

            if (!string.IsNullOrEmpty(request.CustomerEmail))
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.CustomerEmail);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Name = request.CustomerName,
                        Email = request.CustomerEmail,
                        Phone = request.CustomerPhone,
                        RegistrationDate = DateTime.Now
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    customer.Name = request.CustomerName ?? customer.Name;
                    customer.Phone = request.CustomerPhone ?? customer.Phone;
                    await _context.SaveChangesAsync();
                }
                customerCode = customer.Code;
            }
            else
            {
                return BadRequest("Не вказано CustomerEmail.");
            }

            // Перевірка існування автомобіля та отримання його даних
            var vehicle = await _context.Vehicles.FindAsync(request.VehicleCode);
            if (vehicle == null)
            {
                return BadRequest("Автомобіль не знайдений.");
            }

            // Отримуємо CustomerCode із claims
            var customerCodeClaim = User.FindFirst("CustomerCode")?.Value;
            if (string.IsNullOrEmpty(customerCodeClaim) || !int.TryParse(customerCodeClaim, out int authCustomerCode))
            {
                return BadRequest("Користувач не авторизований або CustomerCode відсутній.");
            }

            // Перевіряємо, чи збігається авторизований користувач із тим, хто створює транзакцію
            if (authCustomerCode != customerCode)
            {
                return Forbid("Ви не можете створювати транзакції від імені іншого користувача.");
            }

            // Employee_Code встановлюємо як null, якщо клієнт робить транзакцію
            int? employeeCode = null;

            // Парсимо дату транзакції
            DateTime transactionDate;
            if (!string.IsNullOrEmpty(request.TransactionDate) && DateTime.TryParse(request.TransactionDate, out var parsedDate))
            {
                transactionDate = parsedDate;
            }
            else
            {
                transactionDate = DateTime.Now;
            }

            // Створення нової транзакції
            var transaction = new Transaction
            {
                Customer_Code = customerCode,
                Vehicle_Code = request.VehicleCode,
                Employee_Code = employeeCode,
                TransactionDate = transactionDate,
                Price = request.Price,
                CustomerName = customer.Name,      // Тепер доступно
                CustomerEmail = customer.Email,    // Тепер доступно
                CustomerPhone = customer.Phone,    // Тепер доступно
                VehicleName = vehicle.Name         // Припускаємо, що поле називається Name
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Code }, transaction);
        }

        [HttpPost("create-full")]
        public async Task<IActionResult> CreateTransactionWithItems([FromBody] CreateTransactionRequest request)
        {
            var result = await _transactionService.CreateTransactionAsync(request);
            if (!result)
            {
                return BadRequest("Не вдалося створити угоду або недостатньо автомобілів на складі.");
            }

            return Ok("Угода створена успішно.");
        }

        // PUT: api/Transactions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, Transaction transaction)
        {
            if (id != transaction.Code)
            {
                return BadRequest();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Transactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Transactions/{id}/total
        [HttpGet("{id}/total")]
        public async Task<ActionResult<decimal>> GetTransactionTotalAmount(int id)
        {
            var total = await _context.Transactions
                .Where(t => t.Code == id)
                .Select(t => t.Price)
                .FirstOrDefaultAsync();

            return Ok(total);
        }

        // Допоміжний метод
        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Code == id);
        }
    }
}