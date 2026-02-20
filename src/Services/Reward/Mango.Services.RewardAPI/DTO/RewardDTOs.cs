namespace Mango.Services.RewardAPI.DTO;

public record RewardDto(int Id, string UserId, int Points, List<RewardTransactionDto> Transactions);
public record RewardTransactionDto(int Id, string Type, int Points, string? OrderId, string Description, DateTime CreatedAt);
