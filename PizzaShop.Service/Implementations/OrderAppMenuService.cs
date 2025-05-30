using System.Transactions;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Implementations;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class OrderAppMenuService : IOrderAppMenuService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IOrderTaxMappingRepository _orderTaxMappingRepository;
    private readonly IMenuItemsRepository _menuItemRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitWorkRepository _unitWorkRepository;
    private readonly IOrderedItemModifierMappingRepository _orderItemModifierRepository;
    private readonly IMappingMenuItemsWithModifierRepository _mappingItemModifierRepository;

    public OrderAppMenuService(IFeedbackRepository feedbackRepository, IOrderRepository orderRepository, IMappingMenuItemsWithModifierRepository mappingMenuItemsWithModifierRepository, IMenuItemsRepository menuItemsRepository, IOrderItemRepository orderItemRepository, IOrderTaxMappingRepository orderTaxMappingRepository, IPaymentRepository paymentRepository, IInvoiceRepository invoiceRepository, IOrderedItemModifierMappingRepository orderedItemModifierMappingRepository, ITableRepository tableRepository, IUnitWorkRepository unitWorkRepository)
    {
        _feedbackRepository = feedbackRepository;
        _orderRepository = orderRepository;
        _mappingItemModifierRepository = mappingMenuItemsWithModifierRepository;
        _menuItemRepository = menuItemsRepository;
        _orderItemRepository = orderItemRepository;
        _orderTaxMappingRepository = orderTaxMappingRepository;
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _orderItemModifierRepository = orderedItemModifierMappingRepository;
        _tableRepository = tableRepository;
        _unitWorkRepository = unitWorkRepository;
    }

    public async Task<bool> AddEditOrderComments(int orderId, string comments)
    {
        try
        {
            var order = await _orderRepository.GetOrderById(orderId!);
            if (order != null)
            {
                order.Notes = comments.ToString();
                var isUpdatedOrder = await _orderRepository.UpdateOrder(order);
                if (isUpdatedOrder)
                    return true;
                else
                    return false;
            }
            return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<List<ItemModifierViewModel>> GetItemModifiers(int itemId)
    {
        try
        {
            var menuItemModifiers = await _mappingItemModifierRepository.GetItemModfiers(itemId);
            var model = new List<ItemModifierViewModel>();
            model = menuItemModifiers.Select(item => new ItemModifierViewModel()
            {
                Id = item.Id,
                Name = item.MenuItem.Name ?? "",
                Type = item.MenuItem.Type,
                ModifierGroupId = item.ModifierGroupId,
                ModifierGroupName = item.ModifierGroup.Name ?? "",
                Minselectionrequired = item.MinSelectionRequired ?? 0,
                Maxselectionrequired = item.MaxSelectionAllowed ?? 0,
                ModifierList = item.ModifierGroup.Modifiers.Count() == 0 ? new List<MenuModifierViewModel>() : item.ModifierGroup.Modifiers.GroupBy(m => m.Name).Select(group => group.First()).Select(m => new MenuModifierViewModel()
                {
                    Id = m.Id,
                    Name = m.Name,
                    ModifierGroupId = m.ModifierGroupId,
                    Rate = m.Rate,
                    Quantity = m.Quantity,
                    Description = m.Description,
                }).ToList()   //Where(m => m.IsDeleted == false).
            }).ToList();

            return model;
        }
        catch (Exception e)
        {
            return null!;
        }
    }

    public async Task<bool> MarkAsFavourite(int itemId)
    {
        return await _menuItemRepository.MarkAsFavourite(itemId);
    }

    public async Task<bool> SaveOrder(SaveOrderViewModel model, int userId)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            // await _unitWorkRepository.BeginTransactionAsync();

            var order = await _orderRepository.GetOrderById(model.Id);
            var status = order.OrderStatus;
            order.SubTotal = model.SubTotal;
            order.TotalAmount = model.Total;
            order.Tax = model.Total - model.SubTotal;
            order.OrderStatus = model.Status;
            order.IsSgstSelected = model.IsSgstSelected;
            order.ModifiedBy = userId;
            order.ModifiedAt = DateTime.UtcNow;

            var isOrderUpdated = await _orderRepository.UpdateOrder(order);
            if (!isOrderUpdated)
                return false;

            List<OrderedItem> oldOrderItems = await _orderItemRepository.GetOrderedItemByOrderId(order.Id);
            List<int> newItemsIds = new List<int>();
            List<int> deletedItemIds = new List<int>();

            foreach (var i in model.OrderItems!)
            {
                if (i.Id != -1)
                    newItemsIds.Add((int)i.Id!);
            }

            foreach (var item in model.OrderItems!)
            {
                if (item.Id == -1)
                {
                    var newItem = new OrderedItem()
                    {
                        OrderId = order.Id,
                        Name = item.ItemName ?? "",
                        MenuItemId = (int)item.MenuItemId!,
                        Quantity = (int)item.Quantity!,
                        Rate = item.Rate ?? 0,
                        Amount = item.Rate * item.Quantity,
                        // TotalAmount = (decimal)item.TotalAmount + (decimal)item.ModifiersPrice,
                        TotalAmount = (decimal)item.TotalPrice!,
                        Tax = item.Tax ?? 0,
                        TotalModifierAmount = item.ModifiersPrice,
                        Instruction = item.Comment,
                        ReadyItemQuantity = 0,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        OrderStatus = 3,
                    };

                    var createdOrderItem = await _orderItemRepository.AddOrderItems(newItem);
                    if (createdOrderItem == null)
                        return false;
                    if (item.Modifiers != null)
                    {
                        foreach (var modifier in item.Modifiers!)
                        {
                            OrderedItemModifierMapping newModifier = new OrderedItemModifierMapping()
                            {
                                OrderItemId = createdOrderItem.Id,
                                ModifierId = (int)modifier.Id!,
                                QuantityOfModifier = 1,
                                RateOfModifier = modifier.Rate,
                                TotalAmount = modifier.TotalAmount,
                                IsDeleted = false,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userId,
                            };
                            var isAddedModifier = await _orderItemModifierRepository.AddOrderItemModifiers(newModifier);
                            if (!isAddedModifier)
                                return false;
                        }
                    }
                }
                else
                {
                    var orderItem = await _orderItemRepository.GetOrderedItemById((int)item.Id!);
                    orderItem.Quantity = (int)item.Quantity!;
                    orderItem.Amount = item.Rate * item.Quantity;
                    // TotalAmount = (decimal)item.TotalAmount + (decimal)item.ModifiersPrice,
                    orderItem.TotalAmount = (decimal)item.TotalPrice!;
                    orderItem.Tax = item.Tax ?? 0;
                    orderItem.TotalModifierAmount = item.TotalModifierAmount;
                    if (item.Comment != "" && item.Comment != null)
                        orderItem.Instruction = item.Comment;
                    orderItem.ModifiedAt = DateTime.UtcNow;
                    orderItem.ModifiedBy = userId;
                    var updatedOrderItem = await _orderItemRepository.EditOrderItems(orderItem);
                    if (updatedOrderItem == null)
                        return false;
                }
            }

            foreach (var item in oldOrderItems)
            {
                if (!newItemsIds.Contains(item.Id))
                {
                    deletedItemIds.Add(item.Id);
                }
            }
            if (deletedItemIds.Count() > 0)
            {
                var isDeletedItems = await _orderItemRepository.DeleteOrderItem(deletedItemIds, userId);
                if (isDeletedItems == false)
                    return false;
            }

            foreach (var t in model.OrderTax!)
            {
                OrderTaxMapping newMapping = new OrderTaxMapping
                {
                    OrderId = order.Id,
                    TaxId = t.Id,
                    TaxValue = t.CalculatedTax,
                    IsDeleted = false,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };
                if (status == 1)
                {
                    await _orderTaxMappingRepository.AddOrderTaxMapping(newMapping);
                }
                else
                {
                    await _orderTaxMappingRepository.EditOrderTaxMapping(t.Id, (decimal)t.CalculatedTax!, userId);
                }
            }
            // await _unitWorkRepository.CommitTransactionAsync();
            transaction.Complete();
            return true;
        }
        catch (Exception e)
        {
            // await _unitWorkRepository.RollbackTransactionAsync();
            transaction.Dispose();
            return false;
        }
    }

    public async Task<int> CancelOrder(int orderId, int userId)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            var order = await _orderRepository.OrderDetails(orderId);
            order.SubTotal = 0;
            order.OrderStatus = 6;
            order.TotalAmount = 0;
            order.Tax = 0;
            order.ModifiedBy = userId;
            order.ModifiedAt = DateTime.UtcNow;

            //check ready items
            if (order.OrderedItems.Count() > 0)
            {
                foreach (var item in order.OrderedItems)
                {
                    if (item.ReadyItemQuantity > 0)
                        return 0;
                }
            }

            foreach (var table in order.TableOrderMappings)
            {
                await _tableRepository.UpdateTableStatus((int)table.TableId!, true, userId);
            }

            foreach (var t in order.OrderTaxMappings!)
            {
                await _orderTaxMappingRepository.EditOrderTaxMapping(t.Id, 0, userId);
            }

            await _orderRepository.UpdateOrder(order);
            transaction.Complete();
            return 1;
        }
        catch (Exception e)
        {
            transaction.Dispose();
            return -1;
        }
    }

    public async Task<int> CompleteOrder(int id, int userId)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            // await _unitWorkRepository.BeginTransactionAsync();

            var order = await _orderRepository.OrderDetails(id);

            if (order.OrderedItems.Count() > 0)
            {
                foreach (var item in order.OrderedItems)
                {
                    if (item.Quantity != item.ReadyItemQuantity)
                        return 0;
                }
            }

            await _orderRepository.UpdateOrderStatus(id, 2, userId);

            foreach (var table in order.TableOrderMappings)
            {
                await _tableRepository.UpdateTableStatus((int)table.TableId!, true, userId);
            }

            Invoice invoice = new Invoice
            {
                OrderId = order.Id,
                TotalAmount = (decimal)order.TotalAmount!,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            var createdInvoice = await _invoiceRepository.AddInvoice(invoice, 3, 2);

            // Payment payment = new Payment
            // {
            //     InvoiceId = createdInvoice.Id,
            //     PaymentMethod = 3,
            //     Amount = (decimal)order.TotalAmount!,
            //     Status = 1,
            //     CreatedAt = DateTime.UtcNow,
            //     CreatedBy = userId
            // };
            // await _paymentRepository.AddPayment(payment);

            // await _unitWorkRepository.CommitTransactionAsync();
            transaction.Complete();
            return 1;
        }
        catch (Exception e)
        {
            transaction.Dispose();
            // await _unitWorkRepository.RollbackTransactionAsync();
            return -1;
        }
    }

    public async Task<bool> SaveCustomerFeedBack(int orderId, int? food, int? service, int? ambience, string? comment, int? userId)
    {
        Feedback feedback = new Feedback
        {
            OrderId = orderId,
            Food = food,
            Service = service,
            Ambience = ambience,
            Comments = comment,
            AvgRating = Math.Round((decimal)(service + ambience + food)! / 3, 2),
            CreatedBy = (int)userId!,
            CreatedAt = DateTime.UtcNow
        };
        await _feedbackRepository.AddFeedBack(feedback);
        return true;
    }

    public async Task<bool> CheckItemIsReady(int orderItemId, int userId)
    {
        var orderItem = await _orderItemRepository.GetOrderedItemById(orderItemId);
        if (orderItem.ReadyItemQuantity > 0)
            return true;
        return false;
    }

    public async Task<bool> CheckReadyItemsQuantity(int orderItemId, int quantity)
    {
        var orderItem = await _orderItemRepository.GetOrderedItemById(orderItemId);
        if ((quantity - orderItem.ReadyItemQuantity) >= 0)
            return true;

        return false;
    }

}