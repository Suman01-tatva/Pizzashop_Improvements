using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly PizzashopContext _context;

    public FeedbackRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<bool> AddFeedBack(Feedback feedback)
    {
        try
        {
            // _context.Feedbacks.Add(feedback);
            // await _context.SaveChangesAsync();
            // return true;
            await _context.Database.ExecuteSqlRawAsync("CALL create_feedback({0},{1},{2},{3},{4},{5},{6})", feedback.OrderId, feedback.Food!, feedback.Service!, feedback.Ambience!, feedback.Comments!, feedback.AvgRating!, feedback.CreatedBy!);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false!;
        }
    }
}
