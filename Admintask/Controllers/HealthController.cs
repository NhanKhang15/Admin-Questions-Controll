// Controllers/HealthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AdminPortal.Controllers
{
    public class HealthController : Controller
    {
        private readonly IConfiguration _cfg;
        public HealthController(IConfiguration cfg) => _cfg = cfg;

        [HttpGet("/db-ping")]
        public async Task<IActionResult> DbPing()
        {
            var connStr = _cfg.GetConnectionString("FloriaDb");
            try
            {
                await using var cn = new SqlConnection(connStr);
                await cn.OpenAsync();

                // Test lệnh đơn giản
                using var cmd = new SqlCommand("SELECT DB_NAME()", cn);
                var dbName = (string?)await cmd.ExecuteScalarAsync();

                return Ok(new { ok = true, message = "DB connected", db = dbName });
            }
            catch (SqlException ex)
            {
                // lỗi từ SQL Server (auth, firewall, tcp/ip, v.v.)
                return StatusCode(500, new { ok = false, type = "SqlException", code = ex.Number, ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { ok = false, type = ex.GetType().Name, ex.Message });
            }
        }
    }
}
