using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class SectionRepository : ISectionRepository
{
    private readonly PizzashopContext _context;

    public SectionRepository(PizzashopContext context)
    {
        _context = context;
    }

    public List<SectionViewModel> GetAllSectionsAsync()
    {
        var sections = _context.Sections.Where(s => s.IsDeleted == false)
        .Select(s => new SectionViewModel
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IsDeleted = s.IsDeleted,
            CreatedBy = s.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = s.ModifiedBy,
            ModifiedAt = DateTime.UtcNow
        })
        .OrderBy(s => s.Name).ToList();
        return sections;
    }
    public Section GetSectionById(int sectionId)
    {
        return _context.Sections.FirstOrDefault(s => s.Id == sectionId);
    }

    public async Task<bool> AddSectionAsync(Section section)
    {
        bool isNameUnique = !await _context.Sections
       .AnyAsync(s => s.Name.ToLower() == section.Name.ToLower() && s.IsDeleted == false);
        if (!isNameUnique)
            throw new Exception("Section name must be unique.");

        await _context.Sections.AddAsync(section);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateSectionAsync(Section section)
    {
        bool isNameUnique = !await _context.Sections
            .AnyAsync(s => s.Name.ToLower() == section.Name.ToLower() && s.Id != section.Id && s.IsDeleted == false);
        if (!isNameUnique)
            throw new Exception("Section name must be unique.");

        _context.Sections.Update(section);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<string> DeleteSectionAsync(int id, bool softDelete, int userId)
    {
        var section = GetSectionById(id);
        if (section != null)
        {
            var tables = await _context.Tables.Where(t => t.SectionId == id && t.IsDeleted == false).ToListAsync();
            foreach (var table in tables)
            {
                if (table.IsAvailable == false)
                {
                    return "table is occupied";
                }
                table.IsDeleted = true;
                _context.Tables.Update(table);
            }

            section.IsDeleted = true;
            section.ModifiedBy = userId;
            section.ModifiedAt = DateTime.UtcNow;
            _context.Sections.Update(section);
            await _context.SaveChangesAsync();
            return "success";
        }
        else
        {
            return "section not found";
        }
    }

    public async Task<List<Section>> GetSectionsForWaitingList()
    {
        return await _context.Sections.Where(s => s.IsDeleted == false)
            .Include(s => s.WaitingTokens)
            .ToListAsync();
    }

    public async Task<List<Section>> getSections()
    {
        var sections = await _context.Sections.Where(s => s.IsDeleted == false && s.Tables.Any(t => t.IsAvailable == true)).ToListAsync();
        return sections;
    }
}
