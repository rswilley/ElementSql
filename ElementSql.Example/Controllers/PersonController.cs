using ElementSql.Example.Data;
using Microsoft.AspNetCore.Mvc;

namespace ElementSql.Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IAppStorageManager _storageManager;
        private readonly ILogger<PersonController> _logger;

        public PersonController(
            IAppStorageManager storageManager,
            ILogger<PersonController> logger)
        {
            _storageManager = storageManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (var session = await _storageManager.StartSession())
            {
                var person = await _storageManager.DbContext.PersonRepository.GetByEmailAddress("john@doe.com", session);
                return Ok(person);
            }
        }
    }
}