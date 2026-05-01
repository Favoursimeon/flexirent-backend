namespace FlexiRent.Application.DTOs;

public record CreateReviewDto(
    Guid PropertyId, 
    int Rating, 
    string Comment);

public record VoteReviewDto(
    bool IsHelpful);

public record ReviewDto(
    Guid Id, 
    Guid PropertyId, 
    Guid AuthorId, 
    string AuthorName,
    int Rating, 
    string Comment, 
    int HelpfulVotes, 
    int UnhelpfulVotes,
    DateTime CreatedAt, 
    DateTime? UpdatedAt);
public record RatingSummaryDto(Guid PropertyId, double AverageRating, int ReviewCount);