
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.DAL.Repositories;

namespace FlowGuardMonitoring.BLL.Tests.Services;
// A simple dummy entity used for testing.
public class DummyEntity 
{ 
    public int Id { get; set; } 
}

public class PaginationServiceTests
{
    [Fact]
    public async Task GetPaginatedRecords_Returns_Correct_Data()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<DummyEntity>>();
        int pageNumber = 1;
        int pageSize = 10;
        string sortColumn = "Id";
        string sortDirection = "asc";
        string searchValue = "test";

        // Create a list with exactly 'pageSize' records.
        var expectedRecords = new List<DummyEntity>();
        for (int i = 1; i <= pageSize; i++)
        {
            expectedRecords.Add(new DummyEntity { Id = i });
        }

        // Simulate that there are 25 total records available.
        int totalRecords = 25;
        int expectedTotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        // Setup the repository mock.
        mockRepository
            .Setup(repo => repo.GetPagedAsync(pageNumber, pageSize, sortColumn, sortDirection, searchValue, "test-guid"))
            .ReturnsAsync(expectedRecords);
        mockRepository
            .Setup(repo => repo.GetCount("test-guid", searchValue))
            .Returns(totalRecords);

        var service = new PaginationService<DummyEntity>(mockRepository.Object);

        // Act
        PaginatedResult<DummyEntity> result = await service.GetPaginatedRecords(
            pageNumber, pageSize, sortColumn, sortDirection, searchValue, "test-guid");

        // Assert
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalRecords, result.TotalRecords);
        Assert.Equal(expectedTotalPages, result.TotalPages);
        Assert.Equal(expectedRecords, result.Records);
    }

    [Fact]
    public async Task GetPaginatedRecords_Returns_Empty_List_When_No_Records()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<DummyEntity>>();
        int pageNumber = 1;
        int pageSize = 10;
        string sortColumn = "Id";
        string sortDirection = "asc";
        string searchValue = string.Empty; // no search filter

        var expectedRecords = new List<DummyEntity>(); // empty list
        int totalRecords = 0;

        mockRepository
            .Setup(repo => repo.GetPagedAsync(pageNumber, pageSize, sortColumn, sortDirection, searchValue, "test-guid"))
            .ReturnsAsync(expectedRecords);
        mockRepository
            .Setup(repo => repo.GetCount("test-guid", searchValue))
            .Returns(totalRecords);

        var service = new PaginationService<DummyEntity>(mockRepository.Object);

        // Act
        PaginatedResult<DummyEntity> result = await service.GetPaginatedRecords(
            pageNumber, pageSize, sortColumn, sortDirection, searchValue, "test-guid");

        // Assert
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalRecords, result.TotalRecords);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Records);
    }

    [Fact]
    public async Task GetPaginatedRecords_Calls_Repository_Methods_Once()
    {
        var mockRepository = new Mock<IRepository<DummyEntity>>();
        int pageNumber = 2;
        int pageSize = 5;
        string sortColumn = "Name";
        string sortDirection = "desc";
        string searchValue = "query";

        var expectedRecords = new List<DummyEntity>
        {
            new DummyEntity { Id = 11 },
            new DummyEntity { Id = 12 }
        };
        int totalRecords = 12;

        mockRepository
            .Setup(repo => repo.GetPagedAsync(pageNumber, pageSize, sortColumn, sortDirection, searchValue, "test-guid"))
            .ReturnsAsync(expectedRecords);
        mockRepository
            .Setup(repo => repo.GetCount("test-guid", searchValue))
            .Returns(totalRecords);

        var service = new PaginationService<DummyEntity>(mockRepository.Object);

        await service.GetPaginatedRecords(pageNumber, pageSize, sortColumn, sortDirection, searchValue, "test-guid");

        mockRepository.Verify(
            repo => repo.GetPagedAsync(pageNumber, pageSize, sortColumn, sortDirection, searchValue, "test-guid"),
            Times.Once);
        mockRepository.Verify(
            repo => repo.GetCount("test-guid", searchValue),
            Times.Once);
    }
}