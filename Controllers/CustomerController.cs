using authProject.Model;
using authProject.Service;
using Microsoft.AspNetCore.Mvc;

namespace authProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly ICustomerService service;

        public CustomerController(ICustomerService service)
        {
            this.service = service;
        }

        [HttpGet ("GetAll")]
        public async Task<IActionResult> Get()
        {
            var data = await this.service.Getbycode();
            if(data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpGet("Getbycode")]
        public async Task<IActionResult> Getbycode(string code)
        {
            var data = await this.service.Getbycode(code);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CustomerModel _data)
        {
            var data = await this.service.Create(_data);
          
            return Ok(data);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(CustomerModel _data,string code)
        {
            var data = await this.service.Update(_data,code);

            return Ok(data);
        }

        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove( string code)
        {
            var data = await this.service.Remove(code);

            return Ok(data);
        }
    }
}
