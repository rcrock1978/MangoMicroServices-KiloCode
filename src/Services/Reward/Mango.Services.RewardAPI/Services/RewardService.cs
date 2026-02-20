using Mango.Contracts.Events;
using Mango.MessageBus;
using Mango.Services.RewardAPI.Domain;
using Mango.Services.RewardAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.RewardAPI.Services;

public interface IRewardService
{
    Task<RewardDto?> GetRewardByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> RedeemPointsAsync(string userId, int points, string? orderId, CancellationToken cancellationToken = default);
    Task EarnPointsAsync(string userId, int points, string? orderId, string description, CancellationToken cancellationToken = default);
}

public class RewardService : IRewardService
{
    private readonly RewardDbContext _context;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<RewardService> _logger;

    public RewardService(RewardDbContext context, IMessageBus messageBus, ILogger<RewardService> logger)
    {
        _context = context;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<RewardDto?> GetRewardByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var reward = await _context.Rewards.Include(r => r.Transactions).FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
        if (reward == null) return null;
        return new RewardDto(reward.Id, reward.UserId, reward.Points, reward.Transactions.Select(t => new RewardTransactionDto(t.Id, t.Type, t.Points, t.OrderId, t.Description, t.CreatedAt)).ToList());
    }

    public async Task<int> RedeemPointsAsync(string userId, int points, string? orderId, CancellationToken cancellationToken = default)
    {
        var reward = await _context.Rewards.Include(r => r.Transactions).FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
        if (reward == null || reward.Points < points) throw new InvalidOperationException("Insufficient points");

        reward.Points -= points;
        reward.Transactions.Add(new RewardTransaction { Type = "Redeemed", Points = -points, OrderId = orderId, Description = $"Redeemed {points} points" });
        await _context.SaveChangesAsync(cancellationToken);

        var redeemEvent = new PointsRedeemedEvent(userId, points, points * 0.01m, orderId != null ? Guid.Parse(orderId) : null, DateTime.UtcNow);
        await _messageBus.PublishAsync(redeemEvent, cancellationToken);

        _logger.LogInformation("User {UserId} redeemed {Points} points", userId, points);
        return reward.Points;
    }

    public async Task EarnPointsAsync(string userId, int points, string? orderId, string description, CancellationToken cancellationToken = default)
    {
        var reward = await _context.Rewards.Include(r => r.Transactions).FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
        if (reward == null)
        {
            reward = new Reward { UserId = userId, Points = points };
            _context.Rewards.Add(reward);
        }
        else
        {
            reward.Points += points;
        }

        reward.Transactions.Add(new RewardTransaction { Type = "Earned", Points = points, OrderId = orderId, Description = description });
        await _context.SaveChangesAsync(cancellationToken);

        var earnedEvent = new RewardPointsEarnedEvent(userId, points, 0, orderId != null ? Guid.Parse(orderId) : null, DateTime.UtcNow);
        await _messageBus.PublishAsync(earnedEvent, cancellationToken);

        _logger.LogInformation("User {UserId} earned {Points} points", userId, points);
    }
}
