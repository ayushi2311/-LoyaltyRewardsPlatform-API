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
    public class RewardsController : ControllerBase
    {
        private readonly IRewardsService _rewardsService;

        public RewardsController(IRewardsService rewardsService)
        {
            _rewardsService = rewardsService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateReward([FromBody] CreateRewardDto dto)
        {
            try
            {
                var reward = await _rewardsService.CreateReward(dto);
                return Ok(reward);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateReward(int id, [FromBody] UpdateRewardDto dto)
        {
            try
            {
                var reward = await _rewardsService.UpdateReward(id, dto);
                return Ok(reward);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReward(int id)
        {
            try
            {
                await _rewardsService.DeleteReward(id);
                return Ok("Reward deleted successfully.");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRewards()
        {
            var rewards = await _rewardsService.GetAllRewards();
            return Ok(rewards);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRewardById(int id)
        {
            try
            {
                var reward = await _rewardsService.GetRewardById(id);
                return Ok(reward);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemRewardRequestDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var redemption = await _rewardsService.RedeemReward(userId, dto);
                return Ok(redemption);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("redemptions/{redemptionId}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessRedemption(int redemptionId, [FromBody] ProcessRedemptionDto dto)
        {
            var processedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var redemption = await _rewardsService.ProcessRedemption(redemptionId, dto, processedBy);
                return Ok(redemption);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("redemptions")]
        public async Task<IActionResult> GetRedemptionHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var history = await _rewardsService.GetRedemptionHistory(userId, pageNumber, pageSize);
            return Ok(history);
        }

        [HttpGet("redemptions/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRedemptions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var redemptions = await _rewardsService.GetAllRedemptions(pageNumber, pageSize);
            return Ok(redemptions);
        }
    }
}
