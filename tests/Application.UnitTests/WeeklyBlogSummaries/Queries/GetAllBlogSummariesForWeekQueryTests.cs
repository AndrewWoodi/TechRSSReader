﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Application.UnitTests.Common;
using TechRSSReader.Application.WeeklyBlogSummaries.Queries;
using TechRSSReader.Domain.Entities;
using TechRSSReader.Infrastructure.Persistence;
using Xunit;

namespace TechRSSReader.Application.UnitTests.WeeklyBlogSummaries.Queries
{
    [Collection("QueryTests")]
    public class GetAllBlogSummariesForWeekQueryTests
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetAllBlogSummariesForWeekQueryTests(QueryTestFixture fixture)
        {
            _context = fixture.Context;
            _mapper = fixture.Mapper;
            _currentUserService = fixture.CurrentUserService;
        }

        [Fact]
        public async Task Handle_ReturnsValid()
        {
            var query = new GetAllBlogSummariesForWeekQuery();

            var loggerMock = new Mock<ILogger<GetAllBlogSummariesForWeekQuery.GetAllBlogSummariesForWeekQueryHandler>>();

            var handler = new GetAllBlogSummariesForWeekQuery.GetAllBlogSummariesForWeekQueryHandler(_context, _mapper, _currentUserService, loggerMock.Object);
            WeeklyBlogSummaryViewModel result = await handler.Handle(query, CancellationToken.None);

            List<WeeklyBlogSummary> weeklyBlogSummaries = _context.WeeklyBlogSummaries
                                        .Include(item => item.Blog)
                                        .Where(item => item.BlogId == 1)
                                        .OrderByDescending(item => item.WeekBegins)
                                        .ToList();

            result.ShouldNotBeNull();
            result.WeeklyBlogSummaries.Count.ShouldBe(1);
            WeeklyBlogSummaryDto firstResult = result.WeeklyBlogSummaries[0];
            WeeklyBlogSummary expectedResult = weeklyBlogSummaries[0];

            firstResult.BlogId.ShouldBe(expectedResult.BlogId);
            firstResult.BlogTitle.ShouldBe(expectedResult.Blog.Title);
            firstResult.NewItems.ShouldBe(expectedResult.NewItems);
            firstResult.ItemsExcluded.ShouldBe(expectedResult.ItemsExcluded);
            firstResult.ItemsRatedAtLeastThree.ShouldBe(expectedResult.ItemsRatedAtLeastThree);
            firstResult.ItemsRead.ShouldBe(expectedResult.ItemsRead);

        }
    }
}
