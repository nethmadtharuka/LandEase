using LandEase.Application.DTOs.Auth;
using LandEase.Application.DTOs.Bookings;
using LandEase.Application.DTOs.Services;
using LandEase.Domain.Enums;
using LandEase.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LandEase.Tests.Integration;

public sealed class AuthAndBookingFlowTests : IClassFixture<MySqlContainerFixture>
{
    private readonly MySqlContainerFixture _db;

    public AuthAndBookingFlowTests(MySqlContainerFixture db)
    {
        _db = db;
    }

    [Fact]
    public async Task Register_Login_CreateService_CreateBooking_Succeeds()
    {
        await using var factory = new MySqlApiFactory(_db.ConnectionString);
        using var client = factory.CreateClient();

        // Register helper
        var helperEmail = $"helper_{Guid.NewGuid():N}@test.local";
        var helperRegister = new RegisterDto
        {
            FullName = "Helper One",
            Email = helperEmail,
            Password = "Passw0rd!123",
            Role = UserRole.Helper,
            OriginCountry = "Sri Lanka",
            DestinationCountry = "Australia",
            MigrationStatus = MigrationStatus.Settled,
            PhoneNumber = "0000000000"
        };

        var helperRegisterResp = await client.PostAsJsonAsync("/api/Auth/register", helperRegister);
        if (!helperRegisterResp.IsSuccessStatusCode)
        {
            var bodyText = await helperRegisterResp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Helper register failed: {(int)helperRegisterResp.StatusCode} {helperRegisterResp.StatusCode}\n{bodyText}");
        }
        var helperAuth = await helperRegisterResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        Assert.NotNull(helperAuth?.Data?.Token);

        await MarkUserKycVerifiedAsync(factory, helperEmail);

        // Create service as helper
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", helperAuth!.Data!.Token);

        var createService = new CreateServiceDto
        {
            Title = "Airport Pickup",
            Description = "Pickup from airport to home.",
            Category = ServiceCategory.Transport,
            Price = 50,
            DestinationCountry = "Australia"
        };

        var createServiceResp = await client.PostAsJsonAsync("/api/Services", createService);
        if (!createServiceResp.IsSuccessStatusCode)
        {
            var bodyText = await createServiceResp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Create service failed: {(int)createServiceResp.StatusCode} {createServiceResp.StatusCode}\n{bodyText}");
        }
        var createdService = await createServiceResp.Content.ReadFromJsonAsync<ApiResponse<ServiceListingDto>>();
        Assert.True(createdService?.Success);
        Assert.True(createdService?.Data?.Id > 0);

        // Register migrant
        var migrantEmail = $"migrant_{Guid.NewGuid():N}@test.local";
        var migrantRegister = new RegisterDto
        {
            FullName = "Migrant One",
            Email = migrantEmail,
            Password = "Passw0rd!123",
            Role = UserRole.Migrant,
            OriginCountry = "Sri Lanka",
            DestinationCountry = "Australia",
            MigrationStatus = MigrationStatus.Planning,
            PhoneNumber = "1111111111"
        };

        var migrantRegisterResp = await client.PostAsJsonAsync("/api/Auth/register", migrantRegister);
        if (!migrantRegisterResp.IsSuccessStatusCode)
        {
            var bodyText = await migrantRegisterResp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Migrant register failed: {(int)migrantRegisterResp.StatusCode} {migrantRegisterResp.StatusCode}\n{bodyText}");
        }
        var migrantAuth = await migrantRegisterResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        Assert.NotNull(migrantAuth?.Data?.Token);

        // Create booking as migrant
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", migrantAuth!.Data!.Token);

        var bookingReq = new CreateBookingDto
        {
            ServiceId = createdService!.Data!.Id,
            Notes = "Please contact me before arriving."
        };

        var createBookingResp = await client.PostAsJsonAsync("/api/Bookings", bookingReq);
        if (!createBookingResp.IsSuccessStatusCode)
        {
            var bodyText = await createBookingResp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Create booking failed: {(int)createBookingResp.StatusCode} {createBookingResp.StatusCode}\n{bodyText}");
        }
        var booking = await createBookingResp.Content.ReadFromJsonAsync<ApiResponse<BookingDto>>();

        Assert.True(booking?.Success);
        Assert.Equal(BookingStatus.Requested, booking!.Data!.Status);
        Assert.Equal(createdService.Data.Id, booking.Data.ServiceId);
    }

    [Fact]
    public async Task Booking_OwnService_AsHelper_IsForbidden()
    {
        await using var factory = new MySqlApiFactory(_db.ConnectionString);
        using var client = factory.CreateClient();

        var email = $"helper_{Guid.NewGuid():N}@test.local";
        var register = new RegisterDto
        {
            FullName = "Helper One",
            Email = email,
            Password = "Passw0rd!123",
            Role = UserRole.Helper,
            OriginCountry = "Sri Lanka",
            DestinationCountry = "Australia",
            MigrationStatus = MigrationStatus.Settled
        };

        var regResp = await client.PostAsJsonAsync("/api/Auth/register", register);
        if (!regResp.IsSuccessStatusCode)
        {
            var bodyText = await regResp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Register failed: {(int)regResp.StatusCode} {regResp.StatusCode}\n{bodyText}");
        }
        var auth = await regResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Data!.Token);

        await MarkUserKycVerifiedAsync(factory, email);

        var createServiceResp = await client.PostAsJsonAsync("/api/Services", new CreateServiceDto
        {
            Title = "Test",
            Description = "Test",
            Category = ServiceCategory.Other,
            Price = 10,
            DestinationCountry = "Australia"
        });
        if (!createServiceResp.IsSuccessStatusCode)
        {
            var bodyText = await createServiceResp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Create service failed: {(int)createServiceResp.StatusCode} {createServiceResp.StatusCode}\n{bodyText}");
        }
        var service = await createServiceResp.Content.ReadFromJsonAsync<ApiResponse<ServiceListingDto>>();

        var bookingResp = await client.PostAsJsonAsync("/api/Bookings", new CreateBookingDto
        {
            ServiceId = service!.Data!.Id
        });

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, bookingResp.StatusCode);
    }

    private static async Task MarkUserKycVerifiedAsync(MySqlApiFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.Single(u => u.Email == email.ToLower().Trim());
        user.IsKycVerified = true;
        user.IsEmailVerified = true;
        await db.SaveChangesAsync();
    }
}

