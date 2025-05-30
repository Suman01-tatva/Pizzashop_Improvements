using System.Security.Claims;
using System.Threading.Tasks;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class SectionService : ISectionService
{
    private readonly ISectionRepository _sectionRepository;

    public SectionService(
        ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public List<SectionViewModel> GetAllSections()
    {
        var sections = _sectionRepository.GetAllSectionsAsync();
        return sections;
    }

    public async Task<SectionViewModel?> GetSectionByIdAsync(int id)
    {
        var section = _sectionRepository.GetSectionById(id);
        if (section == null)
            return null;

        return new SectionViewModel
        {
            Id = section.Id,
            Name = section.Name,
            Description = section.Description,
        };
    }

    public async Task<bool> AddSectionAsync(SectionViewModel model, int userId)
    {
        var section = new Section
        {
            Name = model.Name,
            Description = model.Description!,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = userId,
            ModifiedAt = DateTime.UtcNow,
        };
        try
        {
            return await _sectionRepository.AddSectionAsync(section);
        }
        catch (Exception ex)
        {
            throw new Exception("Section name must be unique.", ex);
        }
    }

    public async Task<bool> UpdateSectionAsync(SectionViewModel model, int userId)
    {
        var existingSection = _sectionRepository.GetSectionById(model.Id);
        if (existingSection == null)
        {
            return false;
        }

        existingSection.Name = model.Name;
        existingSection.Description = model.Description!;
        existingSection.ModifiedBy = userId;
        existingSection.ModifiedAt = DateTime.UtcNow;

        try
        {
            return await _sectionRepository.UpdateSectionAsync(existingSection);
        }
        catch (Exception ex)
        {
            throw new Exception("Section name must be unique.", ex);
        }
    }

    public async Task<string> DeleteSectionAsync(int id, bool softDelete, int userId)
    {
        var isSectionDeleted = await _sectionRepository.DeleteSectionAsync(id, softDelete, userId);
        if (isSectionDeleted == "table is occupied")
        {
            return "table is occupied";
        }
        else if (isSectionDeleted == "success")
        {
            return "success";
        }
        else
        {
            return "section not found";
        }
    }
}
