namespace Mango.Contracts.Clients;

public record ServiceUrls(
    string AuthApi,
    string ProductApi,
    string ShoppingCartApi,
    string OrderApi,
    string CouponApi,
    string RewardApi,
    string EmailApi
);
