﻿using AutoMapper;
using Shouldly;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechRSSReader.Application.Blogs.Queries.GetBlogs;
using TechRSSReader.Application.UnitTests.Common;
using TechRSSReader.Infrastructure.Persistence;
using Xunit;

namespace TechRSSReader.Application.UnitTests.Blogs.Queries
{
    [Collection("QueryTests")]
    public class GetBlogsQueryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetBlogsQueryTests(QueryTestFixture fixture)
        {
            _context = fixture.Context;
            _mapper = fixture.Mapper;
        }

        [Fact]
        public async Task Handle_ReturnsCorrect()
        {
            var query = new GetBlogsQuery();

            var handler = new GetBlogsQuery.GetBlogsQueryHandler(_context, _mapper);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldBeOfType<BlogsViewModel>();
            result.Blogs.Count.ShouldBe(1);

            var blog = result.Blogs.First();

            blog.KeywordsToExclude.Count.ShouldBe(1);
            blog.KeywordsToInclude.Count.ShouldBe(1);
        }
    }
}