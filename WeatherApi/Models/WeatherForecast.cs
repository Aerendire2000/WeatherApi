using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApi.Models
{
    // La clase de la plantilla, ahora conectada a la tabla "WeatherForecast"
    [Table("WeatherForecast")]
    public class WeatherForecast
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateOnly Date { get; set; }                  // DateOnly ↔ columna DATE

        public int TemperatureC { get; set; }

        [NotMapped]                                         // no es columna: se calcula
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [MaxLength(50)]
        public string? Summary { get; set; }

        public DateTime FechaRegistro { get; set; }         // dato interno: no sale en el DTO
    }
}
