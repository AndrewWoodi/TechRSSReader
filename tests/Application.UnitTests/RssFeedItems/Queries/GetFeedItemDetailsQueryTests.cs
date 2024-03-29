﻿using AutoMapper;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Application.RssFeedItems.Queries;
using TechRSSReader.Application.UnitTests.Common;
using TechRSSReader.Infrastructure.Persistence;
using Xunit;

namespace TechRSSReader.Application.UnitTests.RssFeedItems.Queries
{
    [Collection("QueryTests")]
    public class GetFeedItemDetailsQueryTests
    {

        private readonly ApplicationDbContext _context;
        private readonly IHtmlSanitizationService _htmlSanitizationService;
        private readonly IMapper _mapper;

        public GetFeedItemDetailsQueryTests(QueryTestFixture fixture)
        {
            _context = fixture.Context;
            _htmlSanitizationService = fixture.HtmlSanitizationService;
            _mapper = fixture.Mapper;
        }

        [Fact]
        public async Task Handle_ReturnsCorrect()
        {

            var query = new GetFeedItemDetailsQuery
            {
                Id = 1
            };

            var handler = new GetFeedItemDetailsQuery.GetFeedItemDetailsQueryHandler(_context, _mapper, _htmlSanitizationService);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldBeOfType<RssFeedItemDetailsDto>();
            result.Id.ShouldBe(1);
            result.FeedItemUserTags.Count.ShouldBe(1);
            result.FeedItemUserTags[0].UserTagText.ShouldBe("Physics");
            result.BlogId.ShouldBeGreaterThan(0);
            string expectedContent = @"<div>Test Content<img src=""test.gif"" style=""margin: 10px""></div>";
            result.Content.ShouldBe(expectedContent);
            string expectedDescription = @"<div>Test Description<img src=""test.gif"" style=""margin: 10px""></div>";
            result.Description.ShouldBe(expectedDescription);

        }
    }
}
