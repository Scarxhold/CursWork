using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CursWork.Models;
using CursWork.DTOs;
using CursWork.Services;
using System.Threading.Tasks;

namespace CursWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly CursWorkContext _context;
        public VehiclesController(CursWorkContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetVehicles()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.Category)
                    .Where(v => v.IsVisible) // Додаємо фільтрацію за видимістю
                    .Select(v => new VehicleDTO
                    {
                        code = v.Code,
                        name = v.Name,
                        price = v.Price,
                        image = v.Image,
                        category_Code = v.Category_Code,
                        categoryName = v.Category != null ? v.Category.Name : null,
                        year = v.Year,
                        mileage = v.Mileage
                    })
                    .ToListAsync();

                if (vehicles == null || !vehicles.Any())
                {
                    return Ok(new { data = new List<VehicleDTO>() });
                }

                return Ok(new { data = vehicles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDTO>> GetVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Category)
                    .Where(v => v.Code == id)
                    .Select(v => new VehicleDTO
                    {
                        code = v.Code,
                        name = v.Name,
                        price = v.Price,
                        image = v.Image,
                        category_Code = v.Category_Code,
                        categoryName = v.Category != null ? v.Category.Name : null,
                        year = v.Year,
                        mileage = v.Mileage
                    })
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {id} not found.");
                }

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<VehicleDTO>> AddVehicle([FromBody] AddVehicleDTO vehicleDto)
        {
            try
            {
                if (vehicleDto == null)
                {
                    return BadRequest("Vehicle data is null.");
                }

                // Логування отриманих даних
                Console.WriteLine($"Received vehicle: Name={vehicleDto.Name}, Category_Code={vehicleDto.Category_Code}, Price={vehicleDto.Price}, Year={vehicleDto.Year}, Mileage={vehicleDto.Mileage}, Image={vehicleDto.Image}");

                // Перевірка, чи існує Category_Code
                var categoryExists = await _context.Categories.AnyAsync(c => c.Code == vehicleDto.Category_Code);
                if (!categoryExists)
                {
                    Console.WriteLine($"Category with Code {vehicleDto.Category_Code} not found.");
                    return BadRequest($"Category with Code {vehicleDto.Category_Code} does not exist.");
                }

                // Мапінг DTO на модель Vehicle
                var vehicle = new Vehicle
                {
                    Name = vehicleDto.Name,
                    Category_Code = vehicleDto.Category_Code,
                    Price = vehicleDto.Price,
                    Year = vehicleDto.Year,
                    Mileage = vehicleDto.Mileage,
                    Image = vehicleDto.Image,
                    IsVisible = true // Новий автомобіль видимий за замовчуванням
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                var createdVehicle = await _context.Vehicles
                    .Include(v => v.Category)
                    .Where(v => v.Code == vehicle.Code)
                    .Select(v => new VehicleDTO
                    {
                        code = v.Code,
                        name = v.Name,
                        price = v.Price,
                        image = v.Image,
                        category_Code = v.Category_Code,
                        categoryName = v.Category != null ? v.Category.Name : null,
                        year = v.Year,
                        mileage = v.Mileage
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Code }, createdVehicle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{code}/hide")]
        public async Task<IActionResult> HideVehicle(int code)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(code);
                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {code} not found.");
                }

                vehicle.IsVisible = false;
                await _context.SaveChangesAsync();

                return Ok($"Vehicle with ID {code} has been hidden.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{code}/edit")]
        public async Task<IActionResult> EditVehicle(int code, [FromBody] Dictionary<string, string> updates)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(code);
                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {code} not found.");
                }

                foreach (var update in updates)
                {
                    switch (update.Key.ToLower())
                    {
                        case "name":
                            vehicle.Name = update.Value;
                            break;
                        case "category_code":
                            vehicle.Category_Code = int.Parse(update.Value);
                            break;
                        case "price":
                            vehicle.Price = decimal.Parse(update.Value);
                            break;
                        case "year":
                            vehicle.Year = int.Parse(update.Value);
                            break;
                        case "mileage":
                            vehicle.Mileage = int.Parse(update.Value);
                            break;
                        case "image":
                            vehicle.Image = update.Value;
                            break;
                        default:
                            return BadRequest($"Invalid field: {update.Key}");
                    }
                }

                await _context.SaveChangesAsync();
                return Ok($"Vehicle with ID {code} updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehicle(int id, Vehicle vehicle)
        {
            try
            {
                if (id != vehicle.Code)
                {
                    return BadRequest("Vehicle ID mismatch.");
                }

                _context.Entry(vehicle).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(id))
                    {
                        return NotFound($"Vehicle with ID {id} not found.");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {id} not found.");
                }

                var inventoryItems = _context.Inventory.Where(i => i.Vehicle_Code == id);
                _context.Inventory.RemoveRange(inventoryItems);

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                return Ok($"Vehicle with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("byprice")]
        public async Task<ActionResult<object>> GetVehiclesByPrice(decimal minPrice, decimal maxPrice)
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.Category)
                    .Where(v => v.Price >= minPrice && v.Price <= maxPrice && v.IsVisible)
                    .Select(v => new VehicleDTO
                    {
                        code = v.Code,
                        name = v.Name,
                        price = v.Price,
                        image = v.Image,
                        category_Code = v.Category_Code,
                        categoryName = v.Category != null ? v.Category.Name : null,
                        year = v.Year,
                        mileage = v.Mileage
                    })
                    .ToListAsync();

                if (vehicles == null || !vehicles.Any())
                {
                    return Ok(new { data = new List<VehicleDTO>() });
                }

                return Ok(new { data = vehicles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("native-expensive")]
        public async Task<ActionResult<object>> GetExpensiveVehicles()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .FromSqlRaw("SELECT * FROM Vehicles WHERE Price > 50000 AND IsVisible = 1")
                    .Include(v => v.Category)
                    .Select(v => new VehicleDTO
                    {
                        code = v.Code,
                        name = v.Name,
                        price = v.Price,
                        image = v.Image,
                        category_Code = v.Category_Code,
                        categoryName = v.Category != null ? v.Category.Name : null,
                        year = v.Year,
                        mileage = v.Mileage
                    })
                    .ToListAsync();

                if (vehicles == null || !vehicles.Any())
                {
                    return Ok(new { data = new List<VehicleDTO>() });
                }

                return Ok(new { data = vehicles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Code == id);
        }
    }
}