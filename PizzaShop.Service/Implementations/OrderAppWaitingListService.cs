using System.Text.Json.Nodes;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class OrderAppWaitingListService : IOrderAppWaitingListService
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IWaitingTokenRepository _waitingRepository;

    private readonly ICustomerRepository _customerRepository;
    private readonly ITableRepository _tableRepository;
    public OrderAppWaitingListService(ISectionRepository sectionRepository, IWaitingTokenRepository waitingRepository, ICustomerRepository customerRepository, ITableRepository tableRepository)
    {
        _sectionRepository = sectionRepository;
        _waitingRepository = waitingRepository;
        _customerRepository = customerRepository;
        _tableRepository = tableRepository;
    }

    public async Task<WaitingTokenListPageViewModel> GetWaitingListPage(int? id)
    {
        WaitingTokenListPageViewModel waitingListPage = new WaitingTokenListPageViewModel();
        List<Section> sections = await _sectionRepository.GetSectionsForWaitingList();
        List<WaitingToken> waitingTokens = await _waitingRepository.GetWaitingTokensBySectionId((int)id!);
        List<SectionViewModel> sectionList = new List<SectionViewModel>();
        List<WaitingTokenViewModel> waitingList = new List<WaitingTokenViewModel>();

        foreach (var s in sections)
        {
            sectionList.Add(new SectionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                WaitingTokenCount = s.WaitingTokens.Where(w => w.IsDeleted == false && w.IsAssigned == false).Count()
            });
        }

        waitingListPage.SectionList = sectionList;

        foreach (var w in waitingTokens)
        {
            string createdDate = w.CreatedAt.HasValue ? w.CreatedAt.Value.ToString("MMMM dd yyyy h:mm tt") : string.Empty;
            string waitingTime = w.CreatedAt.HasValue
                ? DateTime.Now.Subtract(w.CreatedAt.Value).Hours + " hrs " + DateTime.Now.Subtract(w.CreatedAt.Value).Minutes + " min"
                : "0 min";
            waitingList.Add(new WaitingTokenViewModel
            {
                Id = w.Id,
                CreatedAt = createdDate,
                CustomerEmail = w.Customer.Email,
                MobileNumber = w.Customer.Phone,
                CustomerName = w.Customer.Name,
                IsAssigned = w.IsAssigned,
                NoOfPeople = w.NoOfPeople,
                SectionId = w.SectionId,
                WaitingTime = waitingTime
            });
        }

        waitingListPage.WaitingList = waitingList;
        return waitingListPage;
    }

    public async Task<List<SectionViewModel>> GetSectionList()
    {
        List<Section> sections = await _sectionRepository.GetSectionsForWaitingList();
        List<SectionViewModel> sectionList = new List<SectionViewModel>();

        foreach (var s in sections)
        {
            sectionList.Add(new SectionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                WaitingTokenCount = s.WaitingTokens.Where(w => w.IsDeleted == false && w.IsAssigned == false).Count()
            });
        }
        return sectionList;
    }

    public async Task<AddEditWatingTokenViewModel> GetAddEditWaitingToken(int? id)
    {
        AddEditWatingTokenViewModel waitingToken = new AddEditWatingTokenViewModel();
        List<SectionViewModel> sections = _sectionRepository.GetAllSectionsAsync();
        waitingToken.SectionList = sections;
        if (id == null || id == 0)
        {
            return waitingToken;
        }
        else
        {
            WaitingToken Wt = await _waitingRepository.GetWaitingTokensById((int)id);

            waitingToken.Id = Wt.Id;
            waitingToken.Email = Wt.Customer.Email;
            waitingToken.Name = Wt.Customer.Name;
            waitingToken.MobileNumber = Wt.Customer.Phone;
            waitingToken.CustomerId = Wt.Customer.Id;
            waitingToken.SectionId = Wt.SectionId;
            waitingToken.NoOfPersons = Wt.NoOfPeople;
            return waitingToken;
        }
    }

    public async Task<bool> AddEditWaitingToken(AddEditWatingTokenViewModel model, int userId)
    {
        var totalSectionCapacity = _tableRepository.GetAllTablesAsync().Where(t => t.SectionId == model.SectionId && t.IsDeleted == false).Sum(t => t.Capacity);
        if (model.NoOfPersons > totalSectionCapacity)
            return false;
        if (model.Id == null || model.Id == 0)
        {
            Customer isExist = await _customerRepository.GetCustomerByEmail(model.Email!);
            Customer customer;
            if (isExist != null)
            {
                isExist.Name = model.Name;
                isExist.Phone = model.MobileNumber;
                isExist.ModifiedBy = userId;
                isExist.ModifiedAt = DateTime.UtcNow;
                customer = await _customerRepository.UpdateCustomer(isExist);
            }
            else
            {
                var customerModel = new Customer
                {
                    Name = model.Name!,
                    Email = model.Email!,
                    Phone = model.MobileNumber!,
                    CreatedBy = userId,
                };
                customer = await _customerRepository.CreateCustomer(customerModel);
            }

            var addToken = new WaitingToken
            {
                CustomerId = customer.Id,
                NoOfPeople = model.NoOfPersons,
                SectionId = (int)model.SectionId!,
                CreatedBy = userId,
            };
            await _waitingRepository.CreateWaitingToken(addToken);
            return true;
        }
        else
        {
            Customer existingCustomer = await _customerRepository.GetCustomerByEmail(model.Email!);

            existingCustomer.Name = model.Name;
            existingCustomer.Phone = model.MobileNumber;
            existingCustomer.ModifiedBy = userId;

            var customer = await _customerRepository.UpdateCustomer(existingCustomer);
            var updateToken = new WaitingToken
            {
                Id = (int)model.Id,
                CustomerId = customer.Id,
                NoOfPeople = model.NoOfPersons,
                SectionId = (int)model.SectionId!,
                IsAssigned = false,
                ModifiedBy = userId,
                ModifiedAt = DateTime.UtcNow,
            };
            await _waitingRepository.UpdateWaitingToken(updateToken);
            return true;
        }
    }

    public async Task<bool> DeleteWaitingtoken(int id, int userId)
    {
        await _waitingRepository.DeleteWaitingToken(id, userId);
        return true;
    }

    public async Task<List<CustomerSuggestionListViewModel>> SearchCustomerByEmail(string? email)
    {
        List<Customer> customerList = await _customerRepository.SearchCustomer(email);
        List<CustomerSuggestionListViewModel> csList = new List<CustomerSuggestionListViewModel>();

        foreach (var c in customerList)
        {
            csList.Add(new CustomerSuggestionListViewModel
            {
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone
            });
        }
        return csList;
    }

    public async Task<List<Section>> AssignWaitingTokenToTable(int id)
    {
        List<Section> sections = await _sectionRepository.GetSectionsForWaitingList();
        return sections;
    }

    public async Task<AssignTablePageViewModel> GetWaitingTokensById(int id)
    {
        var token = await _waitingRepository.GetWaitingTokensById(id);
        var sectionList = _sectionRepository.GetAllSectionsAsync();

        var modal = new AssignTablePageViewModel
        {
            Id = token.Id,
            SectionId = token.SectionId,
            CustomerId = token.Customer.Id,
            CustomerEmail = token.Customer.Email,
            CustomerName = token.Customer.Name,
            MobileNumber = token.Customer.Phone,
            NoOfPerson = token.NoOfPeople,
            SectionList = sectionList
        };
        return modal;
    }

    public async Task<JsonArray> GetTablesBySectionId(int id)
    {
        List<Table> tables = await _tableRepository.GetAvailableTablesBySectionId(id);
        JsonArray TableList = new JsonArray();
        foreach (var t in tables)
        {
            JsonObject table = new JsonObject
            {
                ["Id"] = t.Id,
                ["Name"] = t.Name,
                ["Capacity"] = t.Capacity,
                ["IsAvailable"] = t.IsAvailable
            };
            TableList.Add(table);
        }
        return TableList;
    }
}
