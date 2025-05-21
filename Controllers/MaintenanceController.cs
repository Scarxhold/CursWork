using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CursWork.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CursWork.Controllers
{
    // Вказуємо маршрут для контролера: всі запити до api/maintenance оброблятимуться тут
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        // Контекст бази даних для взаємодії з таблицями
        private readonly CursWorkContext _context;

        // Конструктор контролера, який приймає контекст бази даних через залежність
        public MaintenanceController(CursWorkContext context)
        {
            _context = context;
        }

        // GET: api/Maintenance
        // Метод для отримання всіх записів про технічне обслуговування
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Успішна відповідь повертає 200 OK
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Помилка сервера повертає 500
        public async Task<ActionResult<IEnumerable<Maintenance>>> GetMaintenance()
        {
            try
            {
                // Завантажуємо всі записи з таблиці Maintenance разом із пов’язаними даними про автомобіль (Vehicle)
                // Include дозволяє завантажити пов’язані дані з іншої таблиці
                var maintenanceList = await _context.Maintenance
                    .Include(m => m.Vehicle) // Завантажуємо пов’язані дані про автомобіль
                    .ToListAsync();

                // Повертаємо список у форматі JSON із кодом 200 OK
                return Ok(maintenanceList);
            }
            catch (Exception ex)
            {
                // У разі помилки (наприклад, проблема з базою даних) повертаємо статус 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Помилка сервера: " + ex.Message);
            }
        }

        // GET: api/Maintenance/5
        // Метод для отримання одного запису про технічне обслуговування за ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Успішна відповідь повертає 200 OK
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Якщо запис не знайдено, повертаємо 404
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Помилка сервера повертає 500
        public async Task<ActionResult<Maintenance>> GetMaintenance(int id)
        {
            try
            {
                // Шукаємо запис за ID, включаючи пов’язані дані про автомобіль
                var maintenance = await _context.Maintenance
                    .Include(m => m.Vehicle)
                    .FirstOrDefaultAsync(m => m.Code == id);

                // Якщо запис не знайдено, повертаємо 404 Not Found
                if (maintenance == null)
                {
                    return NotFound("Запис про технічне обслуговування з ID " + id + " не знайдено.");
                }

                // Повертаємо знайдений запис із кодом 200 OK
                return Ok(maintenance);
            }
            catch (Exception ex)
            {
                // У разі помилки повертаємо статус 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Помилка сервера: " + ex.Message);
            }
        }

        // POST: api/Maintenance
        // Метод для створення нового запису про технічне обслуговування
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Успішне створення повертає 201 Created
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Некоректні дані повертають 400
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Помилка сервера повертає 500
        public async Task<ActionResult<Maintenance>> PostMaintenance(Maintenance maintenance)
        {
            try
            {
                // Перевіряємо, чи коректні вхідні дані (чи відповідають атрибутам моделі)
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Повертаємо 400 із описом помилок
                }

                // Перевіряємо, чи існує автомобіль із вказаним Vehicle_Code
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Code == maintenance.Vehicle_Code);
                if (!vehicleExists)
                {
                    return BadRequest("Автомобіль з ID " + maintenance.Vehicle_Code + " не існує.");
                }

                // Додаємо новий запис до таблиці Maintenance
                _context.Maintenance.Add(maintenance);
                // Зберігаємо зміни в базі даних
                await _context.SaveChangesAsync();

                // Повертаємо 201 Created із посиланням на створений запис
                return CreatedAtAction(nameof(GetMaintenance), new { id = maintenance.Code }, maintenance);
            }
            catch (Exception ex)
            {
                // У разі помилки повертаємо статус 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Помилка сервера: " + ex.Message);
            }
        }

        // PUT: api/Maintenance/5
        // Метод для оновлення існуючого запису про технічне обслуговування
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Успішне оновлення повертає 204 No Content
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Некоректні дані повертають 400
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Якщо запис не знайдено, повертаємо 404
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Помилка сервера повертає 500
        public async Task<IActionResult> PutMaintenance(int id, Maintenance maintenance)
        {
            try
            {
                // Перевіряємо, чи коректні вхідні дані
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Перевіряємо, чи ID у запиті збігається з ID у тілі запиту
                if (id != maintenance.Code)
                {
                    return BadRequest("ID у запиті не збігається з ID у тілі запиту.");
                }

                // Перевіряємо, чи існує запис із таким ID
                if (!MaintenanceExists(id))
                {
                    return NotFound("Запис про технічне обслуговування з ID " + id + " не знайдено.");
                }

                // Перевіряємо, чи існує автомобіль із вказаним Vehicle_Code
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Code == maintenance.Vehicle_Code);
                if (!vehicleExists)
                {
                    return BadRequest("Автомобіль з ID " + maintenance.Vehicle_Code + " не існує.");
                }

                // Оновлюємо запис у базі даних
                _context.Entry(maintenance).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Обробляємо помилку паралельного доступу
                    if (!MaintenanceExists(id))
                    {
                        return NotFound("Запис про технічне обслуговування з ID " + id + " не знайдено.");
                    }
                    else
                    {
                        throw; // Перекидаємо виняток для подальшої обробки
                    }
                }

                // Повертаємо 204 No Content, що означає успішне оновлення
                return NoContent();
            }
            catch (Exception ex)
            {
                // У разі помилки повертаємо статус 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Помилка сервера: " + ex.Message);
            }
        }

        // DELETE: api/Maintenance/5
        // Метод для видалення запису про технічне обслуговування
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Успішне видалення повертає 204 No Content
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Якщо запис не знайдено, повертаємо 404
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Помилка сервера повертає 500
        public async Task<IActionResult> DeleteMaintenance(int id)
        {
            try
            {
                // Шукаємо запис за ID
                var maintenance = await _context.Maintenance.FindAsync(id);
                if (maintenance == null)
                {
                    return NotFound("Запис про технічне обслуговування з ID " + id + " не знайдено.");
                }

                // Видаляємо запис
                _context.Maintenance.Remove(maintenance);
                await _context.SaveChangesAsync();

                // Повертаємо 204 No Content, що означає успішне видалення
                return NoContent();
            }
            catch (Exception ex)
            {
                // У разі помилки повертаємо статус 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Помилка сервера: " + ex.Message);
            }
        }

        // Допоміжний метод для перевірки існування запису
        private bool MaintenanceExists(int id)
        {
            return _context.Maintenance.Any(e => e.Code == id);
        }
    }
}