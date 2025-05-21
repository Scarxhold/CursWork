using CursWork.Services;
using CursWork.Models;
using CursWork.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CursWork.Services
{
    public class TransactionService
    {
        private readonly CursWorkContext _context;

        public TransactionService(CursWorkContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateTransactionAsync(CreateTransactionRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Перевірка EmployeeCode
                if (!request.EmployeeCode.HasValue)
                {
                    throw new InvalidOperationException("EmployeeCode не вказано.");
                }
                var employee = await _context.Employees.FindAsync(request.EmployeeCode.Value);
                if (employee == null)
                {
                    throw new InvalidOperationException("Співробітник не знайдений.");
                }

                // Перевірка CustomerCode
                if (!request.CustomerCode.HasValue)
                {
                    throw new InvalidOperationException("CustomerCode не вказано.");
                }
                var customer = await _context.Customers.FindAsync(request.CustomerCode.Value);
                if (customer == null)
                {
                    throw new InvalidOperationException("Клієнт не знайдений.");
                }

                // Перевірка VehicleCode
                var vehicle = await _context.Vehicles.FindAsync(request.VehicleCode);
                if (vehicle == null)
                {
                    throw new InvalidOperationException("Автомобіль не знайдений.");
                }

                // Створення транзакції
                var transactionEntity = new Transaction
                {
                    Customer_Code = request.CustomerCode.Value,
                    Employee_Code = request.EmployeeCode.Value,
                    Vehicle_Code = request.VehicleCode,
                    TransactionDate = DateTime.UtcNow,
                    Price = request.Price
                };
                _context.Transactions.Add(transactionEntity);
                await _context.SaveChangesAsync();

                // Перевірка і оновлення залишку на складі
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.Vehicle_Code == request.VehicleCode);
                if (inventory == null || inventory.Stock_Quantity < 1)
                {
                    throw new InvalidOperationException("Недостатньо автомобілів на складі.");
                }

                inventory.Stock_Quantity -= 1;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}