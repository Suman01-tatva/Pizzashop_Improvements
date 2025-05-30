using System.Globalization;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;
using PizzaShop.Service.Utils;

namespace PizzaShop.Service.Implementations;

public class DashboardPageService : IDashboardPageService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IWaitingTokenRepository _waitingTokenRepository;

    public DashboardPageService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, ICustomerRepository customerRepository, IWaitingTokenRepository waitingTokenRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _customerRepository = customerRepository;
        _waitingTokenRepository = waitingTokenRepository;
    }

    public async Task<DashboardViewModel> GetDashboardPage(string? TimePeriod, DateTime? startDate, DateTime? endDate)
    {
        DateTime fromDate = DateTime.Now;
        DateTime toDate = DateTime.Now;
        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        if (TimePeriod == "Current Month")
        {
            fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            toDate = fromDate.AddMonths(1).AddDays(-1);
        }
        else if (TimePeriod == "Last 7 days")
        {
            fromDate = DateTime.Now.AddDays(-7);
            // toDate = fromDate.AddDays(7);
        }
        else if (TimePeriod == "This Year")
        {
            fromDate = new DateTime(DateTime.Now.Year, 1, 1);
            toDate = new DateTime(DateTime.Now.Year, 12, 31);
        }
        else if (TimePeriod == "Today")
        {
            fromDate = DateTime.Now.Date;
            toDate = fromDate.AddDays(1);
        }
        else if (TimePeriod == "Last 30 days")
        {
            toDate = DateTime.Now.Date;
            fromDate = toDate.AddDays(-30);
        }
        else
        {
            fromDate = (DateTime)startDate!;
            toDate = (DateTime)endDate!;
        }

        var allOrders = await _orderRepository.GetOrdersForDashboard();
        var orderitems = await _orderItemRepository.GetAllOrderedItems();
        var customers = await _customerRepository.GetAllCustomers();
        var waitingTokens = await _waitingTokenRepository.GetAllWaitingTokens();

        List<Order> orders = allOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= fromDate.Date && o.CreatedAt.Value.Date <= toDate.Date && o.OrderStatus != 6).ToList();
        var topSellingItems = orderitems.Where(i => i.Order.CreatedAt >= fromDate && i.Order.CreatedAt <= toDate && i.Order.OrderStatus != 6)
                            .GroupBy(oi => new { oi.MenuItem.Id, oi.MenuItem.Name, oi.MenuItem.Image })
                            .Select(i => new DashboardItemViewModel
                            {
                                Id = i.Key.Id,
                                Name = i.Key.Name ?? "",
                                ItemImage = i.Key.Image ?? "",
                                OrderCount = i.Select(oi => oi.Order.Id).Distinct().Count()
                            })
                            .OrderByDescending(g => g.OrderCount)
                            .Take(2).ToList() ?? new List<DashboardItemViewModel>();
        var leastSellingItems = orderitems.Where(i => i.Order.CreatedAt >= fromDate && i.Order.CreatedAt <= toDate && i.Order.OrderStatus != 6)
                            .GroupBy(oi => new { oi.MenuItem.Id, oi.MenuItem.Name, oi.MenuItem.Image })
                            .Select(i => new DashboardItemViewModel
                            {
                                Id = i.Key.Id,
                                Name = i.Key.Name ?? "",
                                ItemImage = i.Key.Image ?? "",
                                OrderCount = i.Select(oi => oi.Order.Id).Distinct().Count()
                            })
                            .OrderBy(o => o.OrderCount)
                            .Take(2)
                            .ToList() ?? new List<DashboardItemViewModel>();

        DashboardViewModel DashboardPage = new DashboardViewModel
        {
            TotalOrders = orders.Count(),
            TotalSales = FormatNumberWithCommas(Math.Round(orders.Sum(o => o.TotalAmount ?? 0), 2)),
            AverageOrderValue = FormatNumberWithCommas(orders.Count() > 0 ? Math.Round((decimal)orders.Average(o => o.TotalAmount ?? 0), 2) : 0),
            AverageWaitingTime = waitingTokens.Count(w => w.IsAssigned == true && w.IsDeleted == false && w.CreatedAt.HasValue && w.ModifiedAt.HasValue && w.CreatedAt >= fromDate && w.CreatedAt <= toDate) > 0
                ? Math.Round(
                    waitingTokens
                        .Where(w => w.IsAssigned == true && w.CreatedAt.HasValue && w.ModifiedAt.HasValue && w.CreatedAt >= fromDate && w.CreatedAt <= toDate)
                        .Average(w => (w.ModifiedAt!.Value - w.CreatedAt!.Value).TotalMinutes),
                    2)
                : 0,
            TopSellingItem = topSellingItems.Count() == 0 ? new List<DashboardItemViewModel>() : topSellingItems,
            LeastSellingItem = leastSellingItems.Count() == 0 ? new List<DashboardItemViewModel>() : leastSellingItems,
            NewCustomer = customers.Count(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate),
            WaitingListCount = waitingTokens.Count(w => w.CreatedAt >= fromDate && w.CreatedAt <= toDate && w.IsDeleted == false),
            Revenue = new GraphDetailViewModel
            {
                Labels = TimePeriod switch
                {
                    "Current Month" => Enumerable.Range(1, daysInMonth).Select(day => new DateTime(DateTime.Now.Year, DateTime.Now.Month, day).ToString("dd")).ToList(),
                    "This Week" => Enumerable.Range(0, 7).Select(day => fromDate.AddDays(day).ToString("dddd")).ToList(),
                    "This Year" => Enumerable.Range(1, 12).Select(month => new DateTime(DateTime.Now.Year, month, 1).ToString("MMMM")).ToList(),
                    "Today" => Enumerable.Range(0, 12).Select(hour => $"{9 + hour}:00 - {9 + hour + 1}:00").ToList(),
                    "Last 7 days" => Enumerable.Range(0, 7).Select(day => fromDate.AddDays(day).ToString("dd MMM")).ToList(),
                    "Last 30 days" => Enumerable.Range(0, 30).Select(day => fromDate.AddDays(day).ToString("dd MMM")).ToList(),
                    "Custom Date" => customers.Where(c => c.CreatedAt.HasValue && c.CreatedAt.Value.Date >= fromDate.Date && c.CreatedAt.Value.Date <= toDate.Date)
                    .Select(c => c.CreatedAt!.Value.Date)
                    .Union(
                         orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= fromDate.Date && o.CreatedAt.Value.Date <= toDate.Date)
                            .Select(o => o.CreatedAt!.Value.Date)
                    )
                    .Distinct()
                    .OrderBy(d => d)
                    .Select(d => d.ToString("dd MMM"))
                    .ToList(),
                    _ => new List<string>()
                },
                Values = TimePeriod switch
                {
                    "Current Month" => Enumerable.Range(1, daysInMonth)
                        .Select(day => fromDate.AddDays(day - 1))
                        .GroupJoin(
                            orders,
                            date => date.Date,
                            order => order.CreatedAt.HasValue ? order.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, orders) => (decimal)Math.Round(orders.Sum(o => o.TotalAmount ?? 0), 2)
                        )
                        .ToList(),
                    "This Week" => Enumerable.Range(0, 7)
                        .Select(day => fromDate.AddDays(day))
                        .GroupJoin(
                            orders,
                            date => date.Date,
                            order => order.CreatedAt.HasValue ? order.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, orders) => (decimal)Math.Round(orders.Sum(o => o.TotalAmount ?? 0), 2)
                        )
                        .ToList(),
                    "This Year" => Enumerable.Range(1, 12)
                        .Select(month => new DateTime(DateTime.Now.Year, month, 1))
                        .GroupJoin(
                            orders,
                            date => date.Month,
                            order => order.CreatedAt.HasValue ? order.CreatedAt.Value.Month : 0,
                            (date, orders) => (decimal)Math.Round(orders.Sum(o => o.TotalAmount ?? 0), 2)
                        )
                        .ToList(),
                    "Today" => Enumerable.Range(0, 12)
                        .Select(hour => fromDate.AddHours(8 + hour))
                        .GroupJoin(
                            orders,
                            date => date.Hour,
                            order => order.CreatedAt.HasValue ? order.CreatedAt.Value.Hour : 0,
                            (date, orders) => (decimal)Math.Round((double)orders.Sum(o => o.TotalAmount ?? 0), 2)
                        )
                        .ToList(),
                    "Last 7 days" => Enumerable.Range(0, 7)
                        .Select(day => fromDate.AddDays(day))
                        .GroupJoin(
                            orders,
                            date => date.Date,
                            order => order.CreatedAt.HasValue ? order.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, orders) => (decimal)Math.Round(orders.Sum(o => o.TotalAmount ?? 0), 2)
                        )
                        .ToList(),
                    "Last 30 days" => Enumerable.Range(0, 30)
                        .Select(day => fromDate.AddDays(day))
                        .GroupJoin(
                            orders,
                            date => date.Date,
                            order => order.CreatedAt.HasValue ? order.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, orders) => (decimal)Math.Round(orders.Sum(o => o.TotalAmount ?? 0), 2)
                        )
                        .ToList(),
                    "Custom Date" =>
                        customers.Where(c => c.CreatedAt.HasValue && c.CreatedAt.Value.Date >= fromDate.Date && c.CreatedAt.Value.Date <= toDate.Date)
                                .Select(c => c.CreatedAt!.Value.Date)
                                .Union(
                                    orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= fromDate.Date && o.CreatedAt.Value.Date <= toDate.Date)
                                        .Select(o => o.CreatedAt!.Value.Date)
                                )
                                .Distinct()
                                .OrderBy(d => d)
                                .Select(date =>
                                    (decimal)Math.Round(
                                        orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == date)
                                            .Sum(o => o.TotalAmount ?? 0), 2)
                                )
             .ToList(),
                    _ => new List<decimal>()
                }
            },
            CustomerGrowth = new GraphDetailViewModel
            {
                Labels = TimePeriod switch
                {
                    "Current Month" => Enumerable.Range(1, daysInMonth).Select(day => new DateTime(DateTime.Now.Year, DateTime.Now.Month, day).ToString("dd")).ToList(),
                    "This Week" => Enumerable.Range(0, 7).Select(day => fromDate.AddDays(day).ToString("dddd")).ToList(),
                    "This Year" => Enumerable.Range(1, 12).Select(month => new DateTime(DateTime.Now.Year, month, 1).ToString("MMMM")).ToList(),
                    "Today" => Enumerable.Range(0, 12).Select(hour => $"{9 + hour}:00 - {9 + hour + 1}:00").ToList(),
                    "Last 7 days" => Enumerable.Range(0, 7).Select(day => fromDate.AddDays(day).ToString("dd MMM")).ToList(),
                    "Last 30 days" => Enumerable.Range(0, 30).Select(day => fromDate.AddDays(day).ToString("dd MMM")).ToList(),
                    "Custom Date" =>
                    customers.Where(c => c.CreatedAt.HasValue && c.CreatedAt.Value.Date >= fromDate.Date && c.CreatedAt.Value.Date <= toDate.Date)
                        .Select(c => c.CreatedAt!.Value.Date)
                        .Union(
                            orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= fromDate.Date && o.CreatedAt.Value.Date <= toDate.Date)
                                .Select(o => o.CreatedAt!.Value.Date)
                        )
                        .Distinct()
                        .OrderBy(d => d)
                        .Select(d => d.ToString("dd MMM"))
                        .ToList(),
                    _ => new List<string>()
                },
                Values = TimePeriod switch
                {
                    "Current Month" => Enumerable.Range(1, daysInMonth)
                        .Select(day => fromDate.AddDays(day - 1))
                        .GroupJoin(
                            customers,
                            date => date.Date,
                            customer => customer.CreatedAt.HasValue ? customer.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, customers) => (decimal)customers.Count()
                        )
                        .ToList(),
                    "This Week" => Enumerable.Range(0, 7)
                        .Select(day => fromDate.AddDays(day))
                        .GroupJoin(
                            customers,
                            date => date.Date,
                            customer => customer.CreatedAt.HasValue ? customer.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, customers) => (decimal)customers.Count()
                        )
                        .ToList(),
                    "This Year" => Enumerable.Range(1, 12)
                        .Select(month => new DateTime(DateTime.Now.Year, month, 1))
                        .GroupJoin(
                            customers,
                            date => date.Month,
                            customer => customer.CreatedAt.HasValue ? customer.CreatedAt.Value.Month : 0,
                            (date, customers) => (decimal)customers.Count()
                        )
                        .ToList(),
                    "Today" => Enumerable.Range(0, 12)
                        .Select(hour => fromDate.Date.AddHours(9 + hour))
                        .Select(date =>
                                (decimal)customers.Count(c => c.CreatedAt.HasValue &&
                                       c.CreatedAt.Value.Date == date.Date &&
                                       c.CreatedAt.Value.Hour == date.Hour)
                                )
                        .ToList(),
                    "Last 7 days" => Enumerable.Range(0, 7)
                        .Select(day => fromDate.AddDays(day))
                        .GroupJoin(
                            customers,
                            date => date.Date,
                            customer => customer.CreatedAt.HasValue ? customer.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, customers) => (decimal)customers.Count()
                        )
                        .ToList(),
                    "Last 30 days" => Enumerable.Range(0, 30)
                        .Select(day => fromDate.AddDays(day))
                        .GroupJoin(
                            customers,
                            date => date.Date,
                            customer => customer.CreatedAt.HasValue ? customer.CreatedAt.Value.Date : DateTime.MinValue.Date,
                            (date, customers) => (decimal)customers.Count()
                        )
                        .ToList(),
                    "Custom Date" =>
                    customers.Where(c => c.CreatedAt.HasValue && c.CreatedAt.Value.Date >= fromDate.Date && c.CreatedAt.Value.Date <= toDate.Date)
                            .Select(c => c.CreatedAt!.Value.Date)
                            .Union(
                                orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= fromDate.Date && o.CreatedAt.Value.Date <= toDate.Date)
                                    .Select(o => o.CreatedAt!.Value.Date)
                            )
                            .Distinct()
                            .OrderBy(d => d)
                            .Select(date =>
                                (decimal)customers.Count(c => c.CreatedAt.HasValue && c.CreatedAt.Value.Date == date)
                            )
                            .ToList(),
                    _ => new List<decimal>()
                }
            }
        };

        return DashboardPage;
    }

    private DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }

    public static string FormatNumberWithCommas(decimal number)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:n0}", number);
    }
}