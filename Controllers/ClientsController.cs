//geo-back/Controllers/ClientsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using geoback.Data;
using geoback.Models;

namespace geoback.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<Client>>> GetClients(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    c.Name.Contains(search) || 
                    c.CustomerNumber.Contains(search) ||
                    (c.ProjectName != null && c.ProjectName.Contains(search)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<Client>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clients");
            return StatusCode(500, new { message = "An error occurred while fetching clients" });
        }
    }

    [HttpGet("customer/{customerNumber}")]
    public async Task<ActionResult<Client>> GetByCustomerNumber(string customerNumber)
    {
        try
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber);

            if (client == null)
            {
                return NotFound(new { message = $"Client with customer number {customerNumber} not found" });
            }

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client by customer number: {CustomerNumber}", customerNumber);
            return StatusCode(500, new { message = "An error occurred while fetching the client" });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Client>>> Search([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrEmpty(q))
            {
                return Ok(new List<Client>());
            }

            var clients = await _context.Clients
                .Where(c => c.Name.Contains(q) || 
                           c.CustomerNumber.Contains(q) || 
                           (c.ProjectName != null && c.ProjectName.Contains(q)))
                .Take(10)
                .ToListAsync();

            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching clients with query: {Query}", q);
            return StatusCode(500, new { message = "An error occurred while searching clients" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(Guid id)
    {
        try
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client with ID: {ClientId}", id);
            return StatusCode(500, new { message = "An error occurred while fetching the client" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Client>> CreateClient(Client client)
    {
        try 
        {
            if (string.IsNullOrWhiteSpace(client.CustomerId) ||
                string.IsNullOrWhiteSpace(client.Name) ||
                string.IsNullOrWhiteSpace(client.CustomerNumber) ||
                string.IsNullOrWhiteSpace(client.Email))
            {
                return BadRequest(new { message = "Customer ID, Customer Name, Customer Number, and Customer Email are required." });
            }

            // Check if customer ID already exists
            if (await _context.Clients.AnyAsync(c => c.CustomerId == client.CustomerId))
            {
                return BadRequest(new { message = $"A client with customer ID '{client.CustomerId}' already exists." });
            }

            // Check if customer number already exists
            if (await _context.Clients.AnyAsync(c => c.CustomerNumber == client.CustomerNumber))
            {
                return BadRequest(new { message = $"A client with customer number '{client.CustomerNumber}' already exists." });
            }

            // Generate a new ID if not provided
            if (client.Id == Guid.Empty)
            {
                client.Id = Guid.NewGuid();
            }

            client.CreatedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Client created successfully: {ClientId}", client.Id);

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating client: {ClientData}", new { client.CustomerId, client.CustomerNumber, client.Name });
            
            // Check for unique constraint violations
            if (ex.InnerException?.Message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                return BadRequest(new { message = "A client with this Customer ID or Customer Number already exists." });
            }
            
            return StatusCode(500, new { message = "Database error occurred while creating the client. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client: {ExceptionMessage}", ex.Message);
            return StatusCode(500, new { message = $"An error occurred while creating the client: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(Guid id, Client client)
    {
        if (id != client.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        try
        {
            client.UpdatedAt = DateTime.UtcNow;
            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Client updated successfully: {ClientId}", id);

            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!ClientExists(id))
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }
            else
            {
                _logger.LogError(ex, "Concurrency error updating client: {ClientId}", id);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client: {ClientId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the client" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        try
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Client deleted successfully: {ClientId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client: {ClientId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the client" });
        }
    }

    private bool ClientExists(Guid id)
    {
        return _context.Clients.Any(e => e.Id == id);
    }
}