using AutoMapper;
using Moq;
using System;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Application.Common.Mappings;
using TechRSSReader.Infrastructure.FeedReader.Maps;
using TechRSSReader.Infrastructure.Persistence;
using TechRSSReader.Infrastructure.Services;

namespace TechRSSReader.Application.UnitTests.Common
{
    public class CommandTestBase : IDisposable
    {
        public CommandTestBase()
        {
            Context = ApplicationDbContextFactory.Create();
            
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RssFeedItemMap>();
                cfg.AddMaps(typeof(MappingProfile).Assembly);
            });

            Mapper = configurationProvider.CreateMapper();
            FeedReader = new StubFeedReader(Mapper);
            UserInterestPredictor = new StubUserInterestPredictor();
            HtmlSanitizationService = new HtmlSanitizationService();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(m => m.UserId)
                .Returns("00000000-0000-0000-0000-000000000000");

            CurrentUserService = currentUserServiceMock.Object;
        }

        public ApplicationDbContext Context { get; }
        public IFeedReader FeedReader { get;  }
        public IMapper Mapper { get;  }
        public IUserInterestPredictor UserInterestPredictor { get; }
        public ICurrentUserService CurrentUserService { get; }
        public IHtmlSanitizationService HtmlSanitizationService { get; }

        public void Dispose()
        {
            ApplicationDbContextFactory.Destroy(Context);
            GC.SuppressFinalize(this);
        }
    }
}