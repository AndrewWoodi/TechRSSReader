﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechRSSReader.Application.RssFeedItems.Commands.UpdateFeedItem;
using TechRSSReader.Application.UnitTests.Common;
using Xunit;

namespace TechRSSReader.Application.UnitTests.RssFeedItems.Commands
{
    public class UpdateFeedItemCommandTests : CommandTestBase
    {

        [Fact]
        public async Task Handle_Valid()
        {

            var command = new UpdateFeedItemCommand
            {

                Id = 1,
                UserInterested = true, 
                ReadAlready = false
            };

            var handler = new UpdateFeedItemCommand.UpdateFeedItemCommandHandler(Context, Mapper);


            await handler.Handle(command, CancellationToken.None);

            var rssFeedItem = Context.RssFeedItems.Find(command.Id);

            rssFeedItem.Id.ShouldBe(command.Id);
            rssFeedItem.UserInterested.ShouldBe(command.UserInterested);
            rssFeedItem.ReadAlready.ShouldBe(command.ReadAlready);

        }
    }
}