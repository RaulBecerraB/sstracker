using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace sstracker.Controllers
{
    [Route("api/v1/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly string _csvPath = "Assets/Calendario_SS_2025-08-25_a_2025-08-29.csv";

        [HttpGet("")]
        [SwaggerOperation(Summary = "Health check", Description = "Returns the health status of the service and CSV file availability")]
        public IActionResult GetHealth()
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
