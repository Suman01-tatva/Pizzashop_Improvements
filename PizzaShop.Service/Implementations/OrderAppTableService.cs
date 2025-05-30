using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using Azure.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PizzaShop.Entity.Constants;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Implementations;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class OrderAppTableService : IOrderAppTableService
{

    private readonly IOrderAppTableRepository _orderAppTableRepository;
    private readonly IWaitingTokenRepository _waitingTokenRepository;
    private readonly ISectionRepository _sectionRepository;
    private readonly ITableRepository _tableRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitWorkRepository _unitWorkRepository;
    private readonly ITableOrderRepository _tableOrderRepository;

    public OrderAppTableService(IOrderAppTableRepository orderAppTableRepository, IWaitingTokenRepository waitingTokenRepository, ISectionRepository sectionRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository, ITableOrderRepository tableOrderRepository, ITableRepository tableRepository, IUnitWorkRepository unitWorkRepository)
    {
        _orderAppTableRepository = orderAppTableRepository;
        _waitingTokenRepository = waitingTokenRepository;
        _sectionRepository = sectionRepository;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _tableOrderRepository = tableOrderRepository;
        _tableRepository = tableRepository;
        _unitWorkRepository = unitWorkRepository;
    }

    public async Task<OrderAppTableViewModel> GetTableView()
    {
        var tableView = new OrderAppTableViewModel();
        var section = await _orderAppTableRepository.GetOrderAppTableDetails();
        // var WaitingTokens = await _waitingTokenRepository.GetAllWaitingTokens();
        //1 - pendding , 2 - completed , 3 - inprogress , 5 - Served
        tableView.SectionList = section.Select(s => new OrderAppSectionListViewModel
        {
            Id = s.Id,
            Name = s.Name,
            Available = s.Tables.Where(t => t.IsDeleted == false && t.IsAvailable == true).Count(),
            Assigned = s.Tables.Where(t => t.IsDeleted == false && t.IsAvailable == false)
            .SelectMany(t => t.TableOrderMappings)
            .Count(m => m.Order!.OrderStatus == 1),
            Running = s.Tables.Where(t => t.IsDeleted == false && t.IsAvailable == false)
                .SelectMany(t => t.TableOrderMappings)
                .Count(m => m.Order!.OrderStatus == 3 || m.Order!.OrderStatus == 4),
            TableList = s.Tables.Where(t => t.IsDeleted == false).Select(t => new TableOrderAppViewModel
            {
                Id = t.Id,
                Sectionid = t.SectionId,
                OrderId = t!.TableOrderMappings.Select(t => (int?)t.Order!.Id).LastOrDefault() ?? 0,
                Name = t.Name,
                Capacity = t.Capacity,
                Status = t.IsAvailable,
                TotalAmount = (t.IsAvailable == false ? t!.TableOrderMappings.Select(t => t.Order!.TotalAmount).LastOrDefault() : 0) ?? 0,
                TimeDuration = CalculateDuration(t.TableOrderMappings.Select(t => t.CreatedAt)?.LastOrDefault(), t.IsAvailable) ?? "",
                OrderStatus = CheckOrderStatus(t.IsAvailable, t!.TableOrderMappings.Select(t => t.Order?.OrderStatus).LastOrDefault()!)
            }).ToList()
        }).ToList();

        return tableView;
    }
    private string CalculateDuration(DateTime? createdAt, bool? status)
    {
        if (createdAt == null)
        {
            return "N/A";
        }

        var duration = TimeSpan.Zero;
        if (status == false)
        {
            duration = DateTime.UtcNow - createdAt.Value;
            return $"{duration.Days}d {duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
        }
        return "";
    }

    private string CheckOrderStatus(bool? isAvailable, int? status)
    {
        if (isAvailable == false && status == 1)
        {
            return "Assigned";
        }
        else if (isAvailable == false && (status == 2 || status == 3 || status == 4 || status == 5))
        {
            return "Running";
        }
        else if (isAvailable == true)
        {
            return "Available";
        }
        return "";
    }

    public async Task<AssignTablePageViewModel> GetWaitingTokensBySectionId(int sectionId)
    {
        var tokens = await _waitingTokenRepository.GetWaitingTokensBySectionId(sectionId);
        var sectionList = _sectionRepository.GetAllSectionsAsync();
        var tokenList = tokens.Select(t => new WaitingTokenViewModel
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            CustomerName = t.Customer.Name,
            CustomerEmail = t.Customer.Email,
            MobileNumber = t.Customer.Phone,
            SectionId = t.SectionId,
            // TableId = t.TableId,
            NoOfPeople = t.NoOfPeople,
            IsDeleted = t.IsDeleted,
            IsAssigned = t.IsAssigned
        }).ToList();
        var modal = new AssignTablePageViewModel
        {
            SectionList = sectionList,
            WaitingTokenList = tokenList,
            SectionId = sectionId
        };
        return modal;
    }

    public async Task<string> AssignTableToCustomer(AssignTableViewModel model, int userId)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        var customer = await _customerRepository.GetCustomerByEmail(model.CustomerEmail!);
        if (customer != null)
        {
            try
            {
                var customersOrders = await _orderRepository.GetOrderssByCustomerId(customer.Id);
                //Table is Not assigned
                if (customersOrders.Count() > 0)
                {
                    var isPendingOrder = customersOrders.Any(o => o.OrderStatus == 4 || o.OrderStatus == 3);
                    customersOrders = customersOrders.Where(o => o.OrderStatus == 3).ToList();
                    if (isPendingOrder)
                        return $"customer have already running order:{customersOrders.LastOrDefault()!.Id}";
                }

                if (model.waitingTokenId != null)
                {
                    var updateToken = new WaitingToken
                    {
                        Id = (int)model.waitingTokenId,
                        CustomerId = customer.Id,
                        NoOfPeople = model.NoOfPerson,
                        SectionId = (int)model.SectionId!,
                        IsAssigned = true,
                        ModifiedBy = userId,
                        ModifiedAt = DateTime.UtcNow,
                    };
                    var isUpdateWaitingToken = await _waitingTokenRepository.UpdateWaitingToken(updateToken);
                    if (!isUpdateWaitingToken)
                        return "fail";
                }

                // Update Customer
                customer.Name = model.CustomerName!;
                customer.Phone = model.MobileNumber!;

                var isCustomerUpdate = await _customerRepository.UpdateCustomer(customer);
                if (isCustomerUpdate == null)
                    return "fail";

                //create new Order
                var newOrder = new Order
                {
                    CustomerId = customer.Id,
                    OrderStatus = 1,
                    CreatedBy = userId,
                };

                var CreateNewOrder = await _orderRepository.CreateNewOrder(newOrder);
                if (CreateNewOrder == null)
                    return "fail";

                //Create new TableOrders

                List<TableOrderMapping> tableOrders = new List<TableOrderMapping>();
                var noOfPersons = model.NoOfPerson;
                foreach (var table in model.TableOrders!)
                {
                    tableOrders.Add(new TableOrderMapping
                    {
                        OrderId = CreateNewOrder!.Id,
                        TableId = table.id,
                        NoOfPeople = noOfPersons > table.capacity ? table.capacity : noOfPersons,
                        CreatedBy = userId
                    });
                    noOfPersons -= table.capacity;
                }

                var isCreatedTableOrders = await _tableOrderRepository.CreateNewTableOrders(tableOrders);
                if (!isCreatedTableOrders)
                    return "Error Occured in Create TableOrder";

                foreach (var table in model.TableOrders)
                {
                    var isTableUpdated = await _tableRepository.UpdateTableStatus((int)table.id!, false, userId);
                    if (!isTableUpdated)
                        return "fail";
                }

                transaction.Complete();
                return $"success:{CreateNewOrder.Id}";
            }
            catch (Exception e)
            {
                transaction.Dispose();
                Console.WriteLine(e);
                return null!;
            }
        }
        else
        {
            try
            {
                var newCustomer = new Customer
                {
                    Name = model.CustomerName!,
                    Email = model.CustomerEmail!,
                    Phone = model.MobileNumber!,
                    CreatedBy = userId,
                };
                var isCustomerCreated = await _customerRepository.CreateCustomer(newCustomer);
                if (isCustomerCreated == null)
                {
                    return "fail";
                }

                var createdCustomer = await _customerRepository.GetCustomerByEmail(model.CustomerEmail!);
                var newOrder = new Order
                {
                    CustomerId = createdCustomer.Id,
                    OrderStatus = 1,
                    CreatedBy = userId,
                };

                var CreateNewOrder = await _orderRepository.CreateNewOrder(newOrder);
                if (CreateNewOrder == null)
                    return "fail";

                List<TableOrderMapping> tableOrders = new List<TableOrderMapping>();
                var noOfPersons = model.NoOfPerson;
                foreach (var table in model.TableOrders!)
                {
                    tableOrders.Add(new TableOrderMapping
                    {
                        OrderId = CreateNewOrder!.Id,
                        TableId = table.id,
                        NoOfPeople = noOfPersons > table.capacity ? table.capacity : noOfPersons
                    });
                }

                var isCreatedTableOrders = await _tableOrderRepository.CreateNewTableOrders(tableOrders);
                if (!isCreatedTableOrders)
                    return "fail";

                foreach (var table in model.TableOrders)
                {
                    var isTableUpdated = await _tableRepository.UpdateTableStatus((int)table.id!, false, userId);
                    if (!isTableUpdated)
                        return "fail";
                }

                transaction.Complete();
                return $"success:{CreateNewOrder.Id}";
            }
            catch (Exception e)
            {
                transaction.Dispose();
                Console.WriteLine(e);
                return null!;
            }
        }

    }
}