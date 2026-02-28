using LandEase.Application.DTOs.Services;
using LandEase.Domain.Enums;
using LandEase.Infrastructure;
using LandEase.Tests.Helpers;
using Xunit;

namespace LandEase.Tests.Unit;

public class ServiceListingServiceTests
{
    // ── CreateAsync Tests ─────────────────────────────────────

    [Fact]
    public async Task CreateAsync_KycVerifiedHelper_CreatesService()
    {
        var context = TestDbContextFactory.Create("svc_create_success");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var service = new ServiceListingService(context);
        var dto = new CreateServiceDto
        {
            Title = "Airport Pickup",
            Description = "I will pick you up from the airport.",
            Category = ServiceCategory.Transport,
            Price = 50,
            DestinationCountry = "Australia"
        };

        var result = await service.CreateAsync(helper.Id, dto);

        Assert.NotNull(result);
        Assert.Equal("Airport Pickup", result.Title);
        Assert.Equal(helper.Id, result.ProviderId);
        Assert.Equal("Australia", result.DestinationCountry);
    }

    [Fact]
    public async Task CreateAsync_NotKycVerified_ThrowsException()
    {
        var context = TestDbContextFactory.Create("svc_create_not_kyc");
        var helper = TestDataBuilder.CreateHelper(1);
        helper.IsKycVerified = false;
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var service = new ServiceListingService(context);
        var dto = new CreateServiceDto
        {
            Title = "Airport Pickup",
            Description = "Test",
            Category = ServiceCategory.Transport,
            Price = 50,
            DestinationCountry = "Australia"
        };

        var ex = await Assert.ThrowsAsync<Exception>(
            () => service.CreateAsync(helper.Id, dto));
        Assert.Contains("KYC verification", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ProviderNotFound_ThrowsException()
    {
        var context = TestDbContextFactory.Create("svc_create_no_provider");
        var service = new ServiceListingService(context);
        var dto = new CreateServiceDto
        {
            Title = "Test",
            Description = "Test",
            Category = ServiceCategory.Other,
            Price = 0,
            DestinationCountry = "Australia"
        };

        var ex = await Assert.ThrowsAsync<Exception>(
            () => service.CreateAsync(999, dto));
        Assert.Contains("Provider not found", ex.Message);
    }

    // ── GetByIdAsync Tests ────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingService_ReturnsService()
    {
        var context = TestDbContextFactory.Create("svc_get_by_id");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        var created = await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "City Tour",
            Description = "I will show you around.",
            Category = ServiceCategory.CityOrientation,
            Price = 30,
            DestinationCountry = "Australia"
        });

        var result = await svc.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal("City Tour", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsException()
    {
        var context = TestDbContextFactory.Create("svc_get_not_found");
        var svc = new ServiceListingService(context);

        var ex = await Assert.ThrowsAsync<Exception>(
            () => svc.GetByIdAsync(999));
        Assert.Contains("Service not found", ex.Message);
    }

    // ── GetAllAsync Filtering Tests ───────────────────────────

    [Fact]
    public async Task GetAllAsync_NoFilters_ReturnsAllActive()
    {
        var context = TestDbContextFactory.Create("svc_get_all");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Service One", Description = "Desc",
            Category = ServiceCategory.Transport,
            Price = 10, DestinationCountry = "Australia"
        });
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Service Two", Description = "Desc",
            Category = ServiceCategory.Accommodation,
            Price = 20, DestinationCountry = "Australia"
        });

        var result = await svc.GetAllAsync(new ServiceFilterDto());

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_FilterByCategory_ReturnsCorrectItems()
    {
        var context = TestDbContextFactory.Create("svc_filter_category");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Transport Service", Description = "Desc",
            Category = ServiceCategory.Transport,
            Price = 10, DestinationCountry = "Australia"
        });
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Housing Service", Description = "Desc",
            Category = ServiceCategory.Accommodation,
            Price = 20, DestinationCountry = "Australia"
        });

        var result = await svc.GetAllAsync(new ServiceFilterDto
        {
            Category = ServiceCategory.Transport
        });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Transport Service", result.Items.First().Title);
    }

    [Fact]
    public async Task GetAllAsync_FilterByCountry_ReturnsCorrectItems()
    {
        var context = TestDbContextFactory.Create("svc_filter_country");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Australia Service", Description = "Desc",
            Category = ServiceCategory.Transport,
            Price = 10, DestinationCountry = "Australia"
        });
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Canada Service", Description = "Desc",
            Category = ServiceCategory.Transport,
            Price = 10, DestinationCountry = "Canada"
        });

        var result = await svc.GetAllAsync(new ServiceFilterDto
        {
            DestinationCountry = "Australia"
        });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Australia Service", result.Items.First().Title);
    }

    [Fact]
    public async Task GetAllAsync_FilterByMaxPrice_ReturnsCorrectItems()
    {
        var context = TestDbContextFactory.Create("svc_filter_price");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Cheap Service", Description = "Desc",
            Category = ServiceCategory.Other,
            Price = 20, DestinationCountry = "Australia"
        });
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Expensive Service", Description = "Desc",
            Category = ServiceCategory.Other,
            Price = 200, DestinationCountry = "Australia"
        });

        var result = await svc.GetAllAsync(new ServiceFilterDto { MaxPrice = 50 });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Cheap Service", result.Items.First().Title);
    }

    [Fact]
    public async Task GetAllAsync_SearchTerm_ReturnsMatchingItems()
    {
        var context = TestDbContextFactory.Create("svc_search");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Airport Pickup Service", Description = "Fast pickup.",
            Category = ServiceCategory.Transport,
            Price = 40, DestinationCountry = "Australia"
        });
        await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Grocery Help", Description = "I help with groceries.",
            Category = ServiceCategory.Food,
            Price = 15, DestinationCountry = "Australia"
        });

        var result = await svc.GetAllAsync(new ServiceFilterDto
        {
            SearchTerm = "airport"
        });

        Assert.Equal(1, result.TotalCount);
        Assert.Contains("Airport", result.Items.First().Title);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        var context = TestDbContextFactory.Create("svc_pagination");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        for (int i = 1; i <= 15; i++)
        {
            await svc.CreateAsync(helper.Id, new CreateServiceDto
            {
                Title = $"Service {i}", Description = "Desc",
                Category = ServiceCategory.Other,
                Price = i, DestinationCountry = "Australia"
            });
        }

        var result = await svc.GetAllAsync(new ServiceFilterDto
        {
            Page = 2, PageSize = 10
        });

        Assert.Equal(15, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    // ── UpdateAsync Tests ─────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_Owner_UpdatesSuccessfully()
    {
        var context = TestDbContextFactory.Create("svc_update_success");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        var created = await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "Old Title", Description = "Old Desc",
            Category = ServiceCategory.Other,
            Price = 10, DestinationCountry = "Australia"
        });

        var result = await svc.UpdateAsync(created.Id, helper.Id, new UpdateServiceDto
        {
            Title = "New Title", Description = "New Desc",
            Category = ServiceCategory.Transport,
            Price = 99, DestinationCountry = "Australia",
            IsActive = true
        });

        Assert.Equal("New Title", result.Title);
        Assert.Equal(99, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsException()
    {
        var context = TestDbContextFactory.Create("svc_update_not_owner");
        var helper1 = TestDataBuilder.CreateHelper(1);
        var helper2 = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(helper1, helper2);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        var created = await svc.CreateAsync(helper1.Id, new CreateServiceDto
        {
            Title = "Service", Description = "Desc",
            Category = ServiceCategory.Other,
            Price = 10, DestinationCountry = "Australia"
        });

        var ex = await Assert.ThrowsAsync<Exception>(
            () => svc.UpdateAsync(created.Id, helper2.Id, new UpdateServiceDto
            {
                Title = "Hacked", Description = "Hacked",
                Category = ServiceCategory.Other,
                Price = 0, DestinationCountry = "Australia",
                IsActive = true
            }));
        Assert.Contains("not authorized", ex.Message);
    }

    // ── DeleteAsync Tests ─────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_Owner_DeactivatesService()
    {
        var context = TestDbContextFactory.Create("svc_delete_success");
        var helper = TestDataBuilder.CreateHelper(1);
        context.Users.Add(helper);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        var created = await svc.CreateAsync(helper.Id, new CreateServiceDto
        {
            Title = "To Delete", Description = "Desc",
            Category = ServiceCategory.Other,
            Price = 10, DestinationCountry = "Australia"
        });

        await svc.DeleteAsync(created.Id, helper.Id);

        var allServices = await svc.GetAllAsync(new ServiceFilterDto());
        Assert.Equal(0, allServices.TotalCount);
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsException()
    {
        var context = TestDbContextFactory.Create("svc_delete_not_owner");
        var helper1 = TestDataBuilder.CreateHelper(1);
        var helper2 = TestDataBuilder.CreateHelper(2);
        context.Users.AddRange(helper1, helper2);
        await context.SaveChangesAsync();

        var svc = new ServiceListingService(context);
        var created = await svc.CreateAsync(helper1.Id, new CreateServiceDto
        {
            Title = "Service", Description = "Desc",
            Category = ServiceCategory.Other,
            Price = 10, DestinationCountry = "Australia"
        });

        var ex = await Assert.ThrowsAsync<Exception>(
            () => svc.DeleteAsync(created.Id, helper2.Id));
        Assert.Contains("not authorized", ex.Message);
    }
}