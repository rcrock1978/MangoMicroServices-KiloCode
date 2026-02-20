using Yarp.ReverseProxy.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new RouteConfig
            {
                RouteId = "auth",
                Match = new RouteMatch { Path = "/api/auth/{**catch-all}" },
                ClusterId = "auth-cluster"
            },
            new RouteConfig
            {
                RouteId = "product",
                Match = new RouteMatch { Path = "/api/product/{**catch-all}" },
                ClusterId = "product-cluster"
            },
            new RouteConfig
            {
                RouteId = "cart",
                Match = new RouteMatch { Path = "/api/cart/{**catch-all}" },
                ClusterId = "cart-cluster"
            },
            new RouteConfig
            {
                RouteId = "order",
                Match = new RouteMatch { Path = "/api/order/{**catch-all}" },
                ClusterId = "order-cluster"
            },
            new RouteConfig
            {
                RouteId = "coupon",
                Match = new RouteMatch { Path = "/api/coupon/{**catch-all}" },
                ClusterId = "coupon-cluster"
            },
            new RouteConfig
            {
                RouteId = "reward",
                Match = new RouteMatch { Path = "/api/reward/{**catch-all}" },
                ClusterId = "reward-cluster"
            }
        },
        new[]
        {
            new ClusterConfig
            {
                ClusterId = "auth-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["auth"] = new DestinationConfig { Address = "http://localhost:7001" }
                }
            },
            new ClusterConfig
            {
                ClusterId = "product-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["product"] = new DestinationConfig { Address = "http://localhost:7002" }
                }
            },
            new ClusterConfig
            {
                ClusterId = "cart-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["cart"] = new DestinationConfig { Address = "http://localhost:7003" }
                }
            },
            new ClusterConfig
            {
                ClusterId = "order-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["order"] = new DestinationConfig { Address = "http://localhost:7004" }
                }
            },
            new ClusterConfig
            {
                ClusterId = "coupon-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["coupon"] = new DestinationConfig { Address = "http://localhost:7005" }
                }
            },
            new ClusterConfig
            {
                ClusterId = "reward-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["reward"] = new DestinationConfig { Address = "http://localhost:7006" }
                }
            }
        }
    );

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapReverseProxy();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
