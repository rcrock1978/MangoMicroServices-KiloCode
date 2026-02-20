using Mango.Contracts.Responses;
using Mango.Services.RewardAPI.DTO;
using Mango.Services.RewardAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.RewardAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardController : ControllerBase
{
    private readonly IRewardService _rewardService;
    private readonly ILogger<RewardController> _logger;

    public RewardController(IRewardService rewardService, ILogger<RewardController> logger)
    {
        _rewardService = rewardService;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<RewardDto>>> GetReward(string userId, CancellationToken cancellationToken)
    {
        var reward = await _rewardService.GetRewardByUserIdAsync(userId, cancellationToken);
        if (reward == null)
            return NotFound(ApiResponse<RewardDto>.Fail("Reward not found"));
        return Ok(ApiResponse<RewardDto>.Success(reward));
    }

    [HttpPost("{userId}/redeem")]
    public async Task<ActionResult<ApiResponse<int>>> RedeemPoints(string userId, [FromQuery] int points, [FromQuery] string? orderId, CancellationToken cancellationToken)
    {
        try
        {
            var remainingPoints = await _rewardService.RedeemPointsAsync(userId, points, orderId, cancellationToken);
            return Ok(ApiResponse<int>.Success(remainingPoints, message: "Points redeemed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<int>.Fail(ex.Message));
        }
    }
}
