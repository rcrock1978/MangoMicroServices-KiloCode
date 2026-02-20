using Mango.SharedKernel;

namespace Mango.Services.RewardAPI.Domain;

public class Reward : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<RewardTransaction> Transactions { get; set; } = new();
}

public class RewardTransaction : BaseEntity
{
    public int RewardId { get; set; }
    public Reward? Reward { get; set; }
    public string Type { get; set; } = string.Empty; // "Earned" or "Redeemed"
    public int Points { get; set; }
    public string? OrderId { get; set; }
    public string Description { get; set; } = string.Empty;
}
