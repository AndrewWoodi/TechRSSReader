﻿using AutoMapper;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Application.Common.Models;
using TechRSSReader.Domain.Entities;

namespace TechRSSReader.Application.Blogs.Commands.RetrieveFeedItems
{
    public class RetrieveFeedItemsCommand: IRequest<int>
    {
        public int BlogId { get; set; }

        public class RetrieveFeedItemsCommandHandler : IRequestHandler<RetrieveFeedItemsCommand, int>
        {
            private readonly IApplicationDbContext _context;
            private readonly IFeedReader _feedReader; 

            public RetrieveFeedItemsCommandHandler(IApplicationDbContext context, IFeedReader feedReader)
            {
                _context = context;
                _feedReader = feedReader; 
            }

            public async Task<int> Handle(RetrieveFeedItemsCommand request, CancellationToken cancellationToken)
            {
                Blog rssFeed = _context.Blogs.Find(request.BlogId);
                
                if (rssFeed == null)
                {
                    return 0;
                }

                FeedReadResult feedResponse = await _feedReader.ReadAsync(rssFeed.XmlAddress, cancellationToken);

                foreach (RssFeedItem item in feedResponse.RssFeedItems)
                {
                    var existingItem = _context.RssFeedItems
                                            .Where(rssItem => rssItem.Link == item.Link)
                                            .FirstOrDefault();
                    if (existingItem == null)
                    {
                        item.BlogId = request.BlogId;
                        _context.RssFeedItems.Add(item);
                    }
                }
                return await _context.SaveChangesAsync(cancellationToken);
                
            }
        }
    }
}