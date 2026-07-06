using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Entities.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMI.EmployeeManagement.API.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PositionHistoryController : ControllerBase
    {
        private readonly IPositionHistoryBLL _positionHistoryBLL;

        public PositionHistoryController(IPositionHistoryBLL positionHistoryBLL)
        {
            _positionHistoryBLL = positionHistoryBLL;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployeePositionAsync(int id, UpdatePositionRequest request)
        {
            request.EmployeeId = id;
            var response = await _positionHistoryBLL.UpdateEmployeePositionAsync(request);

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
