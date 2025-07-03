using System.Security.Claims;
using LoyaltyRewardsApi.DTOs;
using LoyaltyRewardsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyRewardsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PointsController : ControllerBase
    {
        private readonly IPointsService _pointsService;

        public PointsController(IPointsService pointsService)
        {
            _pointsService = pointsService;
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPoints([FromBody] AddPointsRequestDto request)
        {
            try
            {
                var transaction = await _pointsService.AddPoints(request);
                return Ok(transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("add-bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBulkPoints([FromBody] BulkPointsRequestDto request)
        {
            try
            {
                var transactions = await _pointsService.AddBulkPoints(request);
                return Ok(transactions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("wallet")]
        public async Task<IActionResult> GetWallet()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var wallet = await _pointsService.GetUserWallet(userId);
                return Ok(wallet);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("wallet/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserWallet(int userId)
        {
            try
            {
                var wallet = await _pointsService.GetUserWallet(userId);
                return Ok(wallet);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var history = await _pointsService.GetTransactionHistory(userId, pageNumber, pageSize);
            return Ok(history);
        }

        [HttpGet("history/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserTransactionHistory(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var history = await _pointsService.GetTransactionHistory(userId, pageNumber, pageSize);
            return Ok(history);
        }

        [HttpGet("all-transactions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTransactions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var transactions = await _pointsService.GetAllTransactions(pageNumber, pageSize);
            return Ok(transactions);
        }
    }
}
