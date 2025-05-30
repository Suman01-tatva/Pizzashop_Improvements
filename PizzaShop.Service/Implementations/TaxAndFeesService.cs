namespace PizzaShop.Service.Implementations;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

public class TaxAndFeesService : ITaxAndFeesService
{
    private readonly ITaxAndFeesRepository _taxesAndFeesRepository;

    public TaxAndFeesService(ITaxAndFeesRepository taxesAndFeesRepository)
    {
        _taxesAndFeesRepository = taxesAndFeesRepository;
    }

    public async Task<bool> AddTax(TaxAndFeesViewModel model)
    {
        var tax = await _taxesAndFeesRepository.GetTaxByName(model.Name);
        if (tax != null)
        {
            return false;
        }

        var newTax = new TaxesAndFee
        {
            Name = model.Name.Trim(),
            Type = model.Type,
            TaxValue = model.TaxValue,
            Percentage = model.Type == true ? model.TaxValue : 0,
            FlatAmount = model.Type == false ? model.TaxValue : 0,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive,
            CreatedBy = 4,
            CreatedAt = DateTime.UtcNow
        };

        return await _taxesAndFeesRepository.AddTax(newTax);
    }

    public async Task<List<TaxesAndFee>> GetAllTaxes()
    {
        return await _taxesAndFeesRepository.GetAllTaxes();
    }

    public async Task<TaxAndFeesViewModel> GetTaxById(int id)
    {
        var tax = await _taxesAndFeesRepository.GetTaxById(id);

        if (tax == null)
        {
            return null;
        }

        var taxViewModel = new TaxAndFeesViewModel
        {
            Id = tax.Id,
            Name = tax.Name,
            IsActive = tax.IsActive ?? true,
            IsDefault = tax.IsDefault ?? true,
            Type = tax.Type,
            TaxValue = tax.TaxValue
        };

        return taxViewModel;
    }

    public async Task<bool> UpdateTax(TaxAndFeesViewModel model)
    {
        var tax = await _taxesAndFeesRepository.GetTaxById(model.Id);

        if (tax != null)
        {
            var taxName = await _taxesAndFeesRepository.IsTaxExist(model.Name, model.Id);

            if (taxName == null)
            {
                tax.Name = model.Name.Trim();
                tax.IsDefault = model.IsDefault;
                tax.IsActive = model.IsActive;
                tax.Type = model.Type;
                tax.TaxValue = model.TaxValue;
                tax.Percentage = model.Type == true ? model.TaxValue : 0;
                tax.FlatAmount = model.Type == false ? model.TaxValue : 0;

                await _taxesAndFeesRepository.UpdateTax(tax);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> DeleteTax(int id)
    {
        var tax = await _taxesAndFeesRepository.GetTaxById(id);

        if (tax != null)
        {
            tax.IsDeleted = true;
            await _taxesAndFeesRepository.UpdateTax(tax);
            return true;
        }

        return false;
    }
}
