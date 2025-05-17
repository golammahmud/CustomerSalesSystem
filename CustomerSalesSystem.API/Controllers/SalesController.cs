using Microsoft.AspNetCore.Mvc;

namespace CustomerSalesSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SalesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetSaleByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return result is not null ? Ok(result) : NotFound();
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSaleCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllSalesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/Sales/filter?customerId=1&date=2025-05-17
        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] int? customerId, [FromQuery] DateTime? date)
        {
            try
            {
                var query = new FilterSalesQuery
                {
                    CustomerId = customerId,
                    Date = date
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception )
            {
                return StatusCode(500, "An error occurred");
                throw;
            }
        }

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteSaleCommand(id);
            await _mediator.Send(command);
            return Ok("Sale deleted.");
        }
    }

}
