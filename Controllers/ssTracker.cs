using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
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
            //var now = new DateTime(2025, 8, 25, 9, 0, 0); // Lunes 25/08/2025 9:00
            var now = GetMexicoNow();

            var lines = System.IO.File.ReadAllLines(_csvPath);
            if (lines.Length < 2) return Ok(new { disponibles = new string[0], mensaje = "CSV vacío" });

            // Map DayOfWeek to CSV day abbreviations used in the file (Mon, Tue, Wed, Thu, Fri)
            var dowMap = new Dictionary<DayOfWeek, string>
            {
                [DayOfWeek.Monday] = "Mon",
                [DayOfWeek.Tuesday] = "Tue",
                [DayOfWeek.Wednesday] = "Wed",
                [DayOfWeek.Thursday] = "Thu",
                [DayOfWeek.Friday] = "Fri",
                [DayOfWeek.Saturday] = "Sat",
                [DayOfWeek.Sunday] = "Sun",
            };
            var todayAbbrev = dowMap[now.DayOfWeek];

            // Buscar bloques donde el 'day' (segunda columna) coincida con el día actual y la hora esté en el rango
            var disponibles = new List<string>();
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                if (cols.Length < 5) continue;
                var dayStr = cols[1].Trim(); // usar el nombre del día en CSV, ej "Mon"
                var startStr = cols[2].Trim();
                var endStr = cols[3].Trim();
                var advisorsStr = cols[4].Trim();

                if (!string.Equals(dayStr, todayAbbrev, StringComparison.OrdinalIgnoreCase)) continue;
                if (!TimeSpan.TryParse(startStr, out var start)) continue;
                if (!TimeSpan.TryParse(endStr, out var end)) continue;

                if (now.TimeOfDay >= start && now.TimeOfDay < end)
                {
                    disponibles.AddRange(advisorsStr.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)));
                }
            }

            return Ok(new { disponibles = disponibles.Distinct().ToArray() });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            var now = GetMexicoNow();
            var info = new Dictionary<string, object>
            {
                ["status"] = "Healthy",
                ["serverTime"] = now,
            };

            try
            {
                var fullPath = System.IO.Path.Combine(AppContext.BaseDirectory, "../", _csvPath).Replace("\\", "/");
                // Try original path first, then the app-relative path inside container
                bool exists = System.IO.File.Exists(_csvPath) || System.IO.File.Exists(fullPath);
                info["csvExists"] = exists;
                if (exists)
                {
                    var pathToUse = System.IO.File.Exists(_csvPath) ? _csvPath : fullPath;
                    var lines = System.IO.File.ReadAllLines(pathToUse);
                    info["csvLines"] = lines.Length;
                    var fi = new System.IO.FileInfo(pathToUse);
                    info["csvLastModified"] = fi.LastWriteTime;
                }
            }
            catch (Exception ex)
            {
                info["csvExists"] = false;
                info["error"] = ex.Message;
            }

            return Ok(info);
        }

        private DateTime GetMexicoNow()
        {
            // Try common timezone IDs across Linux and Windows
            var candidates = new[]
            {
                "America/Mexico_City",
                "Mexico Standard Time",
                "Central Standard Time (Mexico)",
                "Central Standard Time"
            };

            foreach (var id in candidates)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                }
                catch { }
            }

            // Fallback to local server time if none found
            return DateTime.Now;
        }
    }
}
