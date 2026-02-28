using LandEase.Application.DTOs.Bookings;
using LandEase.Application.DTOs.Reviews;
using LandEase.Application.DTOs.Services;
using LandEase.Domain.Enums;
using LandEase.Infrastructure;
using LandEase.Tests.Helpers;
using Xunit;

namespace LandEase.Tests.Unit;

public class ReviewServiceTests
{
    private async Task<(int bookingId, int helperId, int migrantId)>
        CreateCompletedBookingAsync(string dbName)
    {
        var context = TestDbContextFactory.Create(dbName);
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new CreateServiceDto
            {
                Title = "Test Service", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Accepted });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.InProgress });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Completed });

        return (booking.Id, helper.Id, migrant.Id);
    }

    [Fact]
    public async Task CreateAsync_ValidReview_CreatesSuccessfully()
    {
        var context = TestDbContextFactory.Create("review_create_success");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Accepted });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.InProgress });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Completed });

        var reviewSvc = new ReviewService(context);
        var result = await reviewSvc.CreateAsync(migrant.Id,
            new CreateReviewDto
            {
                BookingId = booking.Id,
                Rating = 5,
                Comment = "Excellent service!"
            });

        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        Assert.Equal("Excellent service!", result.Comment);
    }

    [Fact]
    public async Task CreateAsync_NotCompletedBooking_ThrowsException()
    {
        var context = TestDbContextFactory.Create("review_not_completed");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        var reviewSvc = new ReviewService(context);
        var ex = await Assert.ThrowsAsync<Exception>(
            () => reviewSvc.CreateAsync(migrant.Id,
                new CreateReviewDto { BookingId = booking.Id, Rating = 5 }));

        Assert.Contains("completed booking", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_DuplicateReview_ThrowsException()
    {
        var context = TestDbContextFactory.Create("review_duplicate");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Accepted });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.InProgress });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Completed });

        var reviewSvc = new ReviewService(context);
        await reviewSvc.CreateAsync(migrant.Id,
            new CreateReviewDto { BookingId = booking.Id, Rating = 4 });

        var ex = await Assert.ThrowsAsync<Exception>(
            () => reviewSvc.CreateAsync(migrant.Id,
                new CreateReviewDto { BookingId = booking.Id, Rating = 3 }));

        Assert.Contains("already reviewed", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_UpdatesProviderAverageRating()
    {
        var context = TestDbContextFactory.Create("review_updates_rating");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Accepted });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.InProgress });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Completed });

        var reviewSvc = new ReviewService(context);
        await reviewSvc.CreateAsync(migrant.Id,
            new CreateReviewDto { BookingId = booking.Id, Rating = 4 });

        var updatedHelper = await context.Users.FindAsync(helper.Id);
        Assert.Equal(4.00m, updatedHelper!.AverageRating);
    }

    [Fact]
    public async Task CreateAsync_NonReviewer_ThrowsException()
    {
        var context = TestDbContextFactory.Create("review_wrong_user");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        var otherMigrant = TestDataBuilder.CreateMigrant(3);
        context.Users.AddRange(migrant, helper, otherMigrant);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Accepted });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.InProgress });
        await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Completed });

        var reviewSvc = new ReviewService(context);
        var ex = await Assert.ThrowsAsync<Exception>(
            () => reviewSvc.CreateAsync(otherMigrant.Id,
                new CreateReviewDto { BookingId = booking.Id, Rating = 5 }));

        Assert.Contains("Only the migrant who booked", ex.Message);
    }
}