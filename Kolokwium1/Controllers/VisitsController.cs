using Kolokwium1.Models.DTOs;
using Kolokwium1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitsController(IVisitService _visitService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult>  GetVisits(int id)
        {
            try
            { 
                var visit = await _visitService.GetVisit(id);
                return Ok(visit);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> postVisit([FromBody]VisitPostDto visitDto)
        {
            
            try
            {
                await _visitService.PostVisit(visitDto);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
            
            return Ok("Rental posted");
        }
        
        
    }
}
