using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace sstracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ssTracker : ControllerBase
    {
        private readonly string _csvPath = "Assets/Calendario_SS_2025-08-25_a_2025-08-29.csv";

        [HttpGet("disponible-ahora")]
        public IActionResult GetDisponiblesAhora()
        {
            // Para pruebas puedes descomentar la siguiente línea:
            var now = new DateTime(2025, 8, 25, 9, 0, 0); // Lunes 25/08/2025 9:00
            //var now = DateTime.Now;

            var lines = System.IO.File.ReadAllLines(_csvPath);
            if (lines.Length < 2) return Ok(new { disponibles = new string[0], mensaje = "CSV vacío" });

            // Buscar bloques donde la fecha y hora actual estén dentro del rango
            var disponibles = new List<string>();
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                if (cols.Length < 5) continue;
                var dateStr = cols[0].Trim();
                var startStr = cols[2].Trim();
                var endStr = cols[3].Trim();
                var advisorsStr = cols[4].Trim();

                if (!DateTime.TryParse(dateStr, out var fecha)) continue;
                if (!TimeSpan.TryParse(startStr, out var start)) continue;
                if (!TimeSpan.TryParse(endStr, out var end)) continue;

                if (fecha.Date == now.Date && now.TimeOfDay >= start && now.TimeOfDay < end)
                {
                    disponibles.AddRange(advisorsStr.Split(';').Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)));
                }
            }

            return Ok(new { disponibles = disponibles.Distinct().ToArray() });
        }
    }
}
