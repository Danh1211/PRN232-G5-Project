using Microsoft.EntityFrameworkCore;

namespace PRN232_BE.BackgroundJobs
{ 
    public class AutoReleaseOrdersService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoReleaseOrdersService> _logger;

        public AutoReleaseOrdersService(
            IServiceProvider serviceProvider,
            ILogger<AutoReleaseOrdersService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auto Release Orders Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAutoReleaseAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi chạy Auto Release Orders");
                }

                // Chạy lại sau 24 giờ (1 ngày)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task ProcessAutoReleaseAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CloneEbayDb1Context>();

            var cutoffDate = DateTime.UtcNow.AddDays(-21);

            var ordersToRelease = await context.OrderTables
                .Include(o => o.Payments)
                .Include(o => o.Seller)
                .Where(o => o.Status == "shipped"
                         && o.OrderDate <= cutoffDate)   // hoặc dùng ShippedDate nếu bạn có cột này
                .ToListAsync();

            if (!ordersToRelease.Any())
            {
                _logger.LogInformation("Không có order nào cần auto release hôm nay.");
                return;
            }

            int successCount = 0;

            foreach (var order in ordersToRelease)
            {
                try
                {
                    order.Status = "delivered";

                    var payment = order.Payments.FirstOrDefault();
                    if (payment != null && payment.Status != "completed")
                    {
                        payment.Status = "completed";
                        payment.PaidAt = DateTime.UtcNow;

                        if (order.Seller != null)
                        {
                            order.Seller.Balance += payment.Amount;
                        }
                    }

                    successCount++;
                    _logger.LogInformation("Auto release order #{OrderId} thành công cho seller {SellerId}",
                        order.Id, order.SellerId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi auto release order #{OrderId}", order.Id);
                }
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Hoàn tất auto release: {Success}/{Total} orders",
                successCount, ordersToRelease.Count);
        }
    }
}