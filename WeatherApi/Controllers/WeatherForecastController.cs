using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApi.Data;
using WeatherApi.DTOs;
using WeatherApi.Models;

namespace WeatherApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/weatherforecast
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WeatherForecastController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/weatherforecast   o   api/weatherforecast?desde=2026-10-20&hasta=2026-10-25
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
        {
            var consulta = _context.WeatherForecasts.AsQueryable();
            if (desde is not null) consulta = consulta.Where(w => w.Date >= desde);
            if (hasta is not null) consulta = consulta.Where(w => w.Date <= hasta);

            var registrosBD = await consulta.OrderBy(w => w.Date).ToListAsync();

            // Mapeo manual entidad → DTO (como en la diapositiva)
            var dtos = registrosBD.Select(w => AMostrar(w)).ToList();

            return Ok(dtos);
        }

        // GET api/weatherforecast/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var registro = await _context.WeatherForecasts.FirstOrDefaultAsync(w => w.Id == id);
            if (registro is null) return NotFound($"No existe el pronóstico con Id {id}.");

            return Ok(AMostrar(registro));
        }

        // POST api/weatherforecast
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WeatherForecastCrearDto dto)
        {
            if (await _context.WeatherForecasts.AnyAsync(w => w.Date == dto.Date))
                return Conflict($"Ya hay un pronóstico para el {dto.Date}.");

            var registro = new WeatherForecast
            {
                Date = dto.Date!.Value,
                TemperatureC = dto.TemperatureC,
                Summary = dto.Summary ?? CalcularSummary(dto.TemperatureC),
                FechaRegistro = DateTime.Now
            };

            _context.WeatherForecasts.Add(registro);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = registro.Id }, AMostrar(registro));
        }

        // PUT api/weatherforecast/1   (reemplaza todos los datos)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] WeatherForecastCrearDto dto)
        {
            var registro = await _context.WeatherForecasts.FindAsync(id);
            if (registro is null) return NotFound($"No existe el pronóstico con Id {id}.");

            if (await _context.WeatherForecasts.AnyAsync(w => w.Date == dto.Date && w.Id != id))
                return Conflict($"Ya hay otro pronóstico para el {dto.Date}.");

            registro.Date = dto.Date!.Value;
            registro.TemperatureC = dto.TemperatureC;
            registro.Summary = dto.Summary ?? CalcularSummary(dto.TemperatureC);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/weatherforecast/1   (cambia solo lo que llega)
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] WeatherForecastPatchDto dto)
        {
            var registro = await _context.WeatherForecasts.FindAsync(id);
            if (registro is null) return NotFound($"No existe el pronóstico con Id {id}.");

            if (dto.Date is not null)
            {
                if (await _context.WeatherForecasts.AnyAsync(w => w.Date == dto.Date && w.Id != id))
                    return Conflict($"Ya hay otro pronóstico para el {dto.Date}.");
                registro.Date = dto.Date.Value;
            }

            if (dto.TemperatureC is not null)
            {
                registro.TemperatureC = dto.TemperatureC.Value;
                // Si cambia la temperatura y no mandan Summary, se recalcula
                if (dto.Summary is null) registro.Summary = CalcularSummary(registro.TemperatureC);
            }

            if (dto.Summary is not null) registro.Summary = dto.Summary;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/weatherforecast/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var registro = await _context.WeatherForecasts.FindAsync(id);
            if (registro is null) return NotFound($"No existe el pronóstico con Id {id}.");

            _context.WeatherForecasts.Remove(registro);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET api/weatherforecast/comparar-meses?mes1=10&mes2=11   (extra: HashSet)
        [HttpGet("comparar-meses")]
        public async Task<IActionResult> CompararMeses([FromQuery] int mes1, [FromQuery] int mes2)
        {
            if (mes1 is < 1 or > 12 || mes2 is < 1 or > 12)
                return BadRequest("Los meses deben estar entre 1 y 12.");

            var summaries1 = await _context.WeatherForecasts
                .Where(w => w.Date.Month == mes1 && w.Summary != null)
                .Select(w => w.Summary!)
                .ToListAsync();
            var summaries2 = await _context.WeatherForecasts
                .Where(w => w.Date.Month == mes2 && w.Summary != null)
                .Select(w => w.Summary!)
                .ToListAsync();

            // new HashSet<string>(lista) quita los repetidos
            var union = new HashSet<string>(summaries1);
            union.UnionWith(summaries2);                // está en uno O en el otro

            var interseccion = new HashSet<string>(summaries1);
            interseccion.IntersectWith(summaries2);     // está en los dos

            var soloMes1 = new HashSet<string>(summaries1);
            soloMes1.ExceptWith(summaries2);            // está en el mes 1 pero NO en el mes 2

            return Ok(new ComparacionResumenesDto
            {
                Mes1 = new HashSet<string>(summaries1),
                Mes2 = new HashSet<string>(summaries2),
                Union = union,
                Interseccion = interseccion,
                SoloMes1 = soloMes1
            });
        }

        // ---------- Métodos auxiliares ----------

        // Entidad → DTO de salida
        private static WeatherForecastDto AMostrar(WeatherForecast w) => new WeatherForecastDto
        {
            Id = w.Id,
            Date = w.Date,
            TemperatureC = w.TemperatureC,
            TemperatureF = w.TemperatureF,
            Summary = w.Summary
        };

        // Misma lista de la plantilla, pero según la temperatura en vez de al azar
        private static string CalcularSummary(int temperaturaC) => temperaturaC switch
        {
            < 0 => "Freezing",
            <= 5 => "Bracing",
            <= 10 => "Chilly",
            <= 15 => "Cool",
            <= 20 => "Mild",
            <= 25 => "Warm",
            <= 30 => "Balmy",
            <= 35 => "Hot",
            <= 40 => "Sweltering",
            _ => "Scorching"
        };
    }
}
