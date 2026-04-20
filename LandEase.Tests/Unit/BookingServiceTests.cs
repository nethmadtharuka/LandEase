using LandEase.Application.DTOs.Bookings;
using LandEase.Domain.Entities;
using LandEase.Domain.Enums;
using LandEase.Infrastructure;
using LandEase.Tests.Helpers;
using Xunit;

namespace LandEase.Tests.Unit;

public class BookingServiceTests
{
    // ── CreateAsync Tests ─────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidBooking_CreatesSuccessfully()
    {
        var context = TestDbContextFactory.Create("booking_create_success");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new Application.DTOs.Services.CreateServiceDto
            {
                Title = "Airport Pickup",
                Description = "Test",
                Category = ServiceCategory.Transport,
                Price = 50,
                DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var result = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Requested, result.Status);
        Assert.Equal(migrant.Id, result.MigrantId);
        Assert.Equal(listing.Id, result.ServiceId);
    }

    [Fact]
    public async Task CreateAsync_ProviderBooksOwnService_ThrowsException()
    {
        var context = TestDbContextFactory.Create("booking_own_service");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new Application.DTOs.Services.CreateServiceDto
            {
                Title = "Test Service", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);

        var ex = await Assert.ThrowsAsync<LandEase.Application.Exceptions.ConflictException>(
            () => bookingSvc.CreateAsync(helper.Id,
                new CreateBookingDto { ServiceId = listing.Id }));

        Assert.Contains("cannot book your own service", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_DuplicateActiveBooking_ThrowsException()
    {
        var context = TestDbContextFactory.Create("booking_duplicate");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new Application.DTOs.Services.CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        var ex = await Assert.ThrowsAsync<LandEase.Application.Exceptions.ConflictException>(
            () => bookingSvc.CreateAsync(migrant.Id,
                new CreateBookingDto { ServiceId = listing.Id }));

        Assert.Contains("already have an active booking", ex.Message);
    }

    // ── UpdateStatusAsync Tests ───────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ProviderAccepts_UpdatesStatus()
    {
        var context = TestDbContextFactory.Create("booking_accept");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new Application.DTOs.Services.CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        var result = await bookingSvc.UpdateStatusAsync(
            booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Accepted });

        Assert.Equal(BookingStatus.Accepted, result.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_MigrantAccepts_ThrowsException()
    {
        var context = TestDbContextFactory.Create("booking_migrant_accept");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new Application.DTOs.Services.CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        var ex = await Assert.ThrowsAsync<LandEase.Application.Exceptions.ForbiddenException>(
            () => bookingSvc.UpdateStatusAsync(
                booking.Id, migrant.Id,
                new UpdateBookingStatusDto { Status = BookingStatus.Accepted }));

        Assert.Contains("Only the service provider", ex.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_CompleteWithoutInProgress_ThrowsException()
    {
        var context = TestDbContextFactory.Create("booking_complete_wrong_state");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new Application.DTOs.Services.CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        var ex = await Assert.ThrowsAsync<LandEase.Application.Exceptions.ConflictException>(
            () => bookingSvc.UpdateStatusAsync(
                booking.Id, helper.Id,
                new UpdateBookingStatusDto { Status = BookingStatus.Completed }));

        Assert.Contains("in-progress", ex.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_FullLifecycle_Succeeds()
    {
        var context = TestDbContextFactory.Create("booking_full_lifecycle");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var helper = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(migrant, helper);
        await context.SaveChangesAsync();

        var serviceListingSvc = new ServiceListingService(context);
        var listing = await serviceListingSvc.CreateAsync(helper.Id,
            new Application.DTOs.Services.CreateServiceDto
            {
                Title = "Test", Description = "Test",
                Category = ServiceCategory.Other,
                Price = 10, DestinationCountry = "Australia"
            });

        var bookingSvc = new BookingService(context);
        var booking = await bookingSvc.CreateAsync(migrant.Id,
            new CreateBookingDto { ServiceId = listing.Id });

        Assert.Equal(BookingStatus.Requested, booking.Status);

        var accepted = await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Accepted });
        Assert.Equal(BookingStatus.Accepted, accepted.Status);

        var inProgress = await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.InProgress });
        Assert.Equal(BookingStatus.InProgress, inProgress.Status);

        var completed = await bookingSvc.UpdateStatusAsync(booking.Id, helper.Id,
            new UpdateBookingStatusDto { Status = BookingStatus.Completed });
        Assert.Equal(BookingStatus.Completed, completed.Status);
    }
}