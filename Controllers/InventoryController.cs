using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CursWork.Services;
using CursWork.Models;

namespace CursWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly CursWorkContext _context;

        public InventoryController(CursWorkContext context)
        {
            _context = context;
        }

        // GET: api/Inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventories()
        {
            return await _context.Inventory.ToListAsync();
        }

        // GET: api/Inventory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventory(int id)
        {
            var inventory = await _context.Inventory.FindAsync(id);

            if (inventory == null)
            {
                return NotFound();
            }

            return inventory;
        }

        // GET: api/Inventory/by-vehicle/5
        [HttpGet("by-vehicle/{vehicleId}")]
        public async Task<ActionResult<Inventory>> GetInventoryByVehicleId(int vehicleId)
        {
            var inventory = await _context.Inventory
                .FirstOrDefaultAsync(i => i.Vehicle_Code == vehicleId);

            if (inventory == null)
            {
                Console.WriteLine($"No inventory found for Vehicle_Code: {vehicleId}");
                return NotFound(new { message = "Запись не найдена", vehicleId = vehicleId, stockQuantity = 0 });
            }

            Console.WriteLine($"Found inventory for Vehicle_Code: {vehicleId}, Stock_Quantity: {inventory.Stock_Quantity}");
            return Ok(inventory);
        }

        // POST: api/Inventory
        [HttpPost]
        public async Task<ActionResult<Inventory>> PostInventory(Inventory inventory)
        {
            if (inventory.Stock_Quantity < 0)
                return BadRequest("Кількість на складі не може бути від'ємною");

            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventory), new { id = inventory.Code }, inventory);
        }

        // PUT: api/Inventory/by-vehicle/3
        [HttpPut("by-vehicle/{vehicleId}")]
        public async Task<IActionResult> UpdateInventoryByVehicleId(int vehicleId, [FromBody] Inventory inventory)
        {
            if (vehicleId != inventory.Vehicle_Code)
                return BadRequest("Vehicle ID не збігається");

            if (inventory.Stock_Quantity < 0)
                return BadRequest("Кількість на складі не може бути від'ємною");

            var existing = await _context.Inventory.FirstOrDefaultAsync(i => i.Vehicle_Code == vehicleId);
            if (existing == null)
                return NotFound();

            existing.Stock_Quantity = inventory.Stock_Quantity;
            existing.Manufacturer_Code = inventory.Manufacturer_Code;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Inventory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.Inventory.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            _context.Inventory.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventory.Any(e => e.Code == id);
        }
    }
}