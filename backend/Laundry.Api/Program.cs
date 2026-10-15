using Laundry.Application.Orders;
using Laundry.Application.Identity;
using Laundry.Domain.Orders;
using Laundry.Infrastructure.Identity;
using Laundry.Infrastructure.Orders;
using Laundry.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IUserAccountRepository, InMemoryUserAccountRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AuthenticationService>();
var postgresConnection = builder.Configuration.GetConnectionString("LocalPostgres");
if (!string.IsNullOrWhiteSpace(postgresConnection)) builder.Services.AddLaundryPostgres(postgresConnection);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", storage = "development-in-memory" }));

app.MapPost("/api/v1/auth/register", async (RegisterCustomer request, AuthenticationService service, CancellationToken cancellationToken) =>
{
    try
    {
        var account = await service.RegisterCustomerAsync(request, cancellationToken);
        return Results.Created($"/api/v1/users/{account.Id}", new { account.Id, account.WhatsAppNumber, Role = account.Role.ToString(), verification = "OTP belum dikonfigurasi" });
    }
    catch (ArgumentException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["password"] = [exception.Message] }); }
    catch (InvalidOperationException exception) { return Results.Conflict(new { message = exception.Message }); }
});

app.MapPost("/api/v1/auth/login", async (LoginRequest request, AuthenticationService service, CancellationToken cancellationToken) =>
{
    var account = await service.LoginAsync(request, cancellationToken);
    return account is null ? Results.Unauthorized() : Results.Ok(new { account.Id, account.WhatsAppNumber, Role = account.Role.ToString(), message = "Token sesi belum diimplementasikan." });
});

app.MapPost("/api/v1/orders/walk-in", async (CreateWalkInOrder request, OrderService service, CancellationToken cancellationToken) =>
{
    if (request.CustomerId == Guid.Empty || request.UnitPrice < 0 || request.MinimumWeightKg < 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["order"] = ["Data pesanan tidak valid."] });
    var order = await service.CreateWalkInAsync(request, cancellationToken);
    return Results.Created($"/api/v1/orders/{order.Id}", ToResponse(order));
});

app.MapGet("/api/v1/orders/{id:guid}", async (Guid id, IOrderRepository orders, CancellationToken cancellationToken) =>
{
    var order = await orders.FindAsync(id, cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(ToResponse(order));
});

app.MapPost("/api/v1/orders/{id:guid}/actual-quantity", async (Guid id, RecordActualQuantity request, IOrderRepository orders, CancellationToken cancellationToken) =>
{
    var order = await orders.FindAsync(id, cancellationToken);
    if (order is null) return Results.NotFound();
    try { order.RecordActualQuantity(request.Quantity); return Results.Ok(ToResponse(order)); }
    catch (ArgumentOutOfRangeException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["quantity"] = [exception.Message] }); }
});

app.MapPost("/api/v1/orders/{id:guid}/payments/cash", async (Guid id, RecordCashPayment request, IOrderRepository orders, CancellationToken cancellationToken) =>
{
    var order = await orders.FindAsync(id, cancellationToken);
    if (order is null) return Results.NotFound();
    try { order.RecordPayment(new Payment(Guid.NewGuid(), request.Amount, request.IdempotencyKey, true)); return Results.Ok(ToResponse(order)); }
    catch (ArgumentOutOfRangeException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["amount"] = [exception.Message] }); }
    catch (ArgumentException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["payment"] = [exception.Message] }); }
});

app.MapPost("/api/v1/orders/{id:guid}/status", async (Guid id, ChangeOrderStatus request, IOrderRepository orders, CancellationToken cancellationToken) =>
{
    var order = await orders.FindAsync(id, cancellationToken);
    if (order is null) return Results.NotFound();
    try { order.TransitionTo(request.Status); return Results.Ok(ToResponse(order)); }
    catch (InvalidOperationException exception) { return Results.Conflict(new { message = exception.Message }); }
});

app.Run();

static object ToResponse(Order order) => new { order.Id, order.OrderNumber, order.Status, PaymentStatus = order.CurrentPaymentStatus, order.FinalTotal, order.Outstanding, order.ActualQuantity };

sealed record RecordActualQuantity(decimal Quantity);
sealed record RecordCashPayment(decimal Amount, string IdempotencyKey);
sealed record ChangeOrderStatus(OrderStatus Status);
