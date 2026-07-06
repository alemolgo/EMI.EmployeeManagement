using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Responses;
using EMI.EmployeeManagement.Entities.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMI.EmployeeManagement.API.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeBLL _employeeBLL;

        public EmployeesController(IEmployeeBLL employeeBLL)
        {
            _employeeBLL = employeeBLL;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAsync(CreateEmployeeRequest request)
        {
            var response = await _employeeBLL.AddAsync(request);

            if (!response.Success)
            {
                return response.ErrorCode switch
                {
                    ErrorTypeEnum.INVALID_REQUEST => BadRequest(response),
                    ErrorTypeEnum.BUSINESS_ERROR => UnprocessableEntity(response),
                    ErrorTypeEnum.DB_ERROR => StatusCode(500, response),
                    _ => StatusCode(500, response)
                };
            }

            return CreatedAtRoute("GetEmployeeById", new { id = response.Id }, response);
        }

                
        [HttpGet("{id}", Name = "GetEmployeeById")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _employeeBLL.GetByIdAsync(id);

            if (!response.Success)
            {
                return response.ErrorCode switch
                {
                    ErrorTypeEnum.INVALID_ID => BadRequest(response),
                    ErrorTypeEnum.NOT_FOUND => NotFound(response),
                    ErrorTypeEnum.BUSINESS_ERROR => UnprocessableEntity(response),
                    ErrorTypeEnum.DB_ERROR => StatusCode(500, response),
                    _ => StatusCode(500, response)
                };
            }

            return Ok(response);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _employeeBLL.GetAllAsync();

            if (!response.Success)
                return StatusCode(500, response);

            return Ok(response);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var response = await _employeeBLL.DeleteAsync(id);

            if (!response.Success)
            {
                return response.ErrorCode switch
                {
                    ErrorTypeEnum.INVALID_ID => BadRequest(response),
                    ErrorTypeEnum.NOT_FOUND => NotFound(response),
                    ErrorTypeEnum.BUSINESS_ERROR => UnprocessableEntity(response),
                    ErrorTypeEnum.DB_ERROR => StatusCode(500, response),
                    _ => StatusCode(500, response)
                };
            }

            return NoContent();
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateEmployeeRequest request)
        {
            request.Id = id;
            var response = await _employeeBLL.UpdateAsync(request);


            if (!response.Success)
            {
                return response.ErrorCode switch
                {
                    ErrorTypeEnum.INVALID_REQUEST => BadRequest(response),
                    ErrorTypeEnum.INVALID_ID => BadRequest(response),
                    ErrorTypeEnum.NOT_FOUND => NotFound(response),
                    ErrorTypeEnum.BUSINESS_ERROR => UnprocessableEntity(response),
                    ErrorTypeEnum.DB_ERROR => StatusCode(500, response),
                    _ => StatusCode(500, response)
                };
            }

            return NoContent();
        }
    }
}