using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class KotService : IKotService
{

    private readonly IMenuCategoryRepository _menuCategoryRepository;
    private readonly IOrderAppKotRepository _orderAppKotRepository;

    public KotService(IMenuCategoryRepository menuCategoryRepository, IOrderAppKotRepository orderAppKotRepository)
    {
        _menuCategoryRepository = menuCategoryRepository;
        _orderAppKotRepository = orderAppKotRepository;
    }

    public async Task<KotViewModel> GetKotDetails(int? categoryId, int? statusId)
    {
        if (statusId == null)
            statusId = 3;
        if (categoryId == null)
            categoryId = 0;

        var KotData = await _orderAppKotRepository.GetKotDetails(categoryId, statusId);

        var categories = await _menuCategoryRepository.GetAllMenuCategoriesAsync();

        var TableDetails = KotData
            .GroupBy(o => o.Id)
            .Select(orderGroup =>
            {
                var firstOrder = orderGroup.First();

                // Group by Ordered_Item_Id within this order
                var orderedItems = orderGroup
                    .GroupBy(i => i.Ordered_Item_Id)
                    .Select(itemGroup =>
                    {
                        var firstItem = itemGroup.First();

                        // Filter out deleted items
                        if (firstItem.Is_Deleted) return null;

                        // Modifiers for this item
                        var modifiers = itemGroup
                            .Where(x => x.Modifier_Id != null)
                            .Select(x => new ModifierList
                            {
                                Name = x.Modifier_Name
                            })
                            .Distinct()
                            .ToList();

                        return new ItemList
                        {
                            OrderItemId = firstItem.Ordered_Item_Id,
                            ItemName = firstItem.Menu_Item_Name,
                            Quantity = statusId == 2 ? firstItem.Ready_Item_Quantity : (firstItem.Quantity - firstItem.Ready_Item_Quantity),
                            ReadyItemQuantity = firstItem.Ready_Item_Quantity,
                            Instruction = firstItem.Instruction ?? "",
                            ModifierList = modifiers
                        };
                    })
                    .Where(x => x != null)
                    .ToList();

                if (!orderedItems.Any()) return null;

                return new KotTableDetailViewModel
                {
                    orderNo = firstOrder.Id,
                    OrderDuration = CalculateDuration(firstOrder.Created_At, statusId, firstOrder.Modified_At),
                    SectionName = firstOrder.Section_Name,
                    TableName = firstOrder.Table_Name,
                    OrderInstructions = firstOrder.Notes,
                    CategoryId = categoryId,
                    status = statusId,
                    OrderedItems = orderedItems!
                };
            })
            .Where(x => x != null)
            .ToList();

        var KotDetails = new KotViewModel
        {
            Categories = categories,
            TableDetails = TableDetails!
        };
        return KotDetails;
    }

    private string CalculateDuration(DateTime? createdAt, int? statusId, DateTime? modifiedAt)
    {
        if (createdAt == null)
        {
            return "N/A"; // Return "N/A" if CreatedAt is null
        }

        var duration = TimeSpan.Zero;
        if (statusId == 3)
        {
            duration = DateTime.UtcNow - createdAt.Value;
        }
        else if (modifiedAt != null)
        {
            duration = modifiedAt!.Value - createdAt.Value;
        }
        else
        {
            duration = DateTime.UtcNow - createdAt.Value;
        }
        return $"{duration.Days}d {duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
    }

    public async Task<bool> MarkAsPreparedItems(List<UpdateOrderItems> items, int userId)
    {
        var isPrepared = await _orderAppKotRepository.MarkAsPreparedItems(items, userId);
        if (isPrepared)
            return true;
        else
            return false;
    }

    public async Task<bool> MarkAsInProgress(List<UpdateOrderItems> items, int userId)
    {
        var isInProgress = await _orderAppKotRepository.MarkAsInProgress(items, userId);
        if (isInProgress)
            return true;
        else
            return false;
    }
}
