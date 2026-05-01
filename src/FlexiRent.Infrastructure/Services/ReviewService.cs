using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure.Services;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto dto);
    Task<IEnumerable<ReviewDto>> GetByPropertyAsync(Guid propertyId);
    Task<IEnumerable<ReviewDto>> GetByUserAsync(Guid userId);
    Task<ReviewDto> VoteAsync(Guid userId, Guid reviewId, VoteReviewDto dto);
    Task DeleteReviewAsync(Guid userId, Guid reviewId);
    Task<RatingSummaryDto> GetRatingSummaryAsync(Guid propertyId);
}

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db) => _db = db;

    public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto dto)
    {
        var review = new Review
        {
            Id = Guid.NewGuid(),
            PropertyId = dto.PropertyId,
            AuthorId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        var author = await _db.Profiles.FindAsync(userId);
        return MapToDto(review, author?.FullName ?? "Unknown", 0, 0);
    }

    public async Task<IEnumerable<ReviewDto>> GetByPropertyAsync(Guid propertyId)
    {
        var reviews = await _db.Reviews
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .Where(r => r.PropertyId == propertyId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return reviews.Select(r => MapToDto(r, r.Author.FullName,
            r.Votes.Count(v => v.IsHelpful),
            r.Votes.Count(v => !v.IsHelpful)));
    }

    public async Task<IEnumerable<ReviewDto>> GetByUserAsync(Guid userId)
    {
        var reviews = await _db.Reviews
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .Where(r => r.AuthorId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return reviews.Select(r => MapToDto(r, r.Author.FullName,
            r.Votes.Count(v => v.IsHelpful),
            r.Votes.Count(v => !v.IsHelpful)));
    }

    public async Task<ReviewDto> VoteAsync(Guid userId, Guid reviewId, VoteReviewDto dto)
    {
        var review = await _db.Reviews
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .FirstOrDefaultAsync(r => r.Id == reviewId)
            ?? throw new ApplicationException("Review not found.");

        var existing = review.Votes.FirstOrDefault(v => v.UserId == userId);
        if (existing != null)
        {
            existing.IsHelpful = dto.IsHelpful;
        }
        else
        {
            _db.ReviewVotes.Add(new ReviewVote
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                UserId = userId,
                IsHelpful = dto.IsHelpful,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
        await _db.Entry(review).Collection(r => r.Votes).LoadAsync();
        return MapToDto(review, review.Author.FullName,
            review.Votes.Count(v => v.IsHelpful),
            review.Votes.Count(v => !v.IsHelpful));
    }

    public async Task DeleteReviewAsync(Guid userId, Guid reviewId)
    {
        var review = await _db.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.AuthorId == userId)
            ?? throw new ApplicationException("Review not found.");
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
    }

    public async Task<RatingSummaryDto> GetRatingSummaryAsync(Guid propertyId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.PropertyId == propertyId)
            .ToListAsync();
        var avg = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;
        return new RatingSummaryDto(propertyId, Math.Round(avg, 2), reviews.Count);
    }

    private static ReviewDto MapToDto(Review r, string authorName, int helpful, int unhelpful) => new(
        r.Id, r.PropertyId, r.AuthorId, authorName,
        r.Rating, r.Comment, helpful, unhelpful,
        r.CreatedAt, r.UpdatedAt);
}