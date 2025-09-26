using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace sstracker.Controllers
{
    [Route("api/v1/schedule")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly string _csvPath = "Assets/Calendario_SS_2025-08-25_a_2025-08-29.csv";

        [HttpGet("advisors/available")]
        [SwaggerOperation(Summary = "Get currently available advisors", Description = "Returns a list of advisors who are available at the current time")]
        public IActionResult GetAvailableAdvisors()
        {
            // Para pruebas puedes descomentar la siguiente línea:
            //var now = new DateTime(2025, 8, 25, 9, 0, 0); // Lunes 25/08/2025 9:00
            var now = GetMexicoNow();

            var lines = System.IO.File.ReadAllLines(_csvPath);
            if (lines.Length < 2) return Ok(new { available = new string[0], message = "Empty CSV file" });

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

            // Buscar bloques donde el 'day' (primera columna) coincida con el día actual y la hora esté en el rango
            var disponibles = new List<string>();
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                if (cols.Length < 4) continue; // Ahora son 4 columnas: day,start,end,advisors
                var dayStr = cols[0].Trim(); // día ahora es la primera columna
                var startStr = cols[1].Trim();
                var endStr = cols[2].Trim();
                var advisorsStr = cols[3].Trim();

                if (!string.Equals(dayStr, todayAbbrev, StringComparison.OrdinalIgnoreCase)) continue;
                if (!TimeSpan.TryParse(startStr, out var start)) continue;
                if (!TimeSpan.TryParse(endStr, out var end)) continue;

                if (now.TimeOfDay >= start && now.TimeOfDay < end)
                {
                    disponibles.AddRange(advisorsStr.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)));
                }
            }

        return Ok(new { available = disponibles.Distinct().ToArray() });
    }

    [HttpGet("")]
    [SwaggerOperation(Summary = "Get complete schedule", Description = "Returns the complete schedule with all time slots and advisors")]
    public IActionResult GetSchedule()
    {
        try
        {
            var lines = System.IO.File.ReadAllLines(_csvPath);
            if (lines.Length < 2) return Ok(new { entries = new object[0], message = "Empty CSV file" });

            var entries = new List<object>();
            
            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                if (cols.Length < 4) continue;
                
                var dayStr = cols[0].Trim();
                var startStr = cols[1].Trim();
                var endStr = cols[2].Trim();
                var advisorsStr = cols[3].Trim();
                
                if (string.IsNullOrWhiteSpace(dayStr)) continue;
                
                entries.Add(new
                {
                    day = dayStr,
                    start = startStr,
                    end = endStr,
                    advisors = advisorsStr,
                    advisorsList = advisorsStr.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToArray()
                });
            }

            return Ok(new { 
                entries = entries.ToArray(),
                totalEntries = entries.Count,
                lastModified = System.IO.File.GetLastWriteTime(_csvPath)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
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
