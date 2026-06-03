using ElementSql.Example.Data;
using ElementSql.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElementSql.Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IStorageManager _storageManager;
        private readonly ILogger<PersonController> _logger;

        public PersonController(
            StorageManager storageManager,
            ILogger<PersonController> logger)
        {
            _storageManager = storageManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var session = await _storageManager.StartSessionAsync();
            var person = await session.FirstOrDefaultWhereAsync<Person>(p => p.EmailAddress == "john@doe.com");

            return Ok(person);
        }
    }
}
