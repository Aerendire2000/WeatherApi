using System.ComponentModel.DataAnnotations;

namespace WeatherApi.DTOs
{
    // SALIDA: lo que ve el cliente (sin FechaRegistro)
    public class WeatherForecastDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public string? Summary { get; set; }
    }

    // ENTRADA para POST y PUT (sin Id ni FechaRegistro: esos los pone el servidor)
    public class WeatherForecastCrearDto
    {
        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateOnly? Date { get; set; }

        [Range(-60, 60, ErrorMessage = "La temperatura debe estar entre -60 y 60 °C.")]
        public int TemperatureC { get; set; }

        // Opcional: si no se envía, el controller lo calcula según la temperatura
        [RegularExpression("Freezing|Bracing|Chilly|Cool|Mild|Warm|Balmy|Hot|Sweltering|Scorching",
            ErrorMessage = "Summary no válido.")]
        public string? Summary { get; set; }
    }

    // ENTRADA para PATCH: todo opcional, solo se cambia lo que llega
    public class WeatherForecastPatchDto
    {
        public DateOnly? Date { get; set; }

        [Range(-60, 60, ErrorMessage = "La temperatura debe estar entre -60 y 60 °C.")]
        public int? TemperatureC { get; set; }

        [RegularExpression("Freezing|Bracing|Chilly|Cool|Mild|Warm|Balmy|Hot|Sweltering|Scorching",
            ErrorMessage = "Summary no válido.")]
        public string? Summary { get; set; }
    }

    // SALIDA del extra con HashSet
    public class ComparacionResumenesDto
    {
        public HashSet<string> Mes1 { get; set; } = new();
        public HashSet<string> Mes2 { get; set; } = new();
        public HashSet<string> Union { get; set; } = new();
        public HashSet<string> Interseccion { get; set; } = new();
        public HashSet<string> SoloMes1 { get; set; } = new();
    }
}
