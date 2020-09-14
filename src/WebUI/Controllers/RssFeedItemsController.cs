﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechRSSReader.Application.Common.Exceptions;
using TechRSSReader.Application.RssFeedItems.Commands.UpdateFeedItem;
using TechRSSReader.Application.RssFeedItems.Queries;

namespace TechRSSReader.WebUI.Controllers
{

    [Authorize]
    public class RssFeedItemsController: ApiController
    {
        [HttpGet("[action]")]
        public async Task<RssFeedItemDto> GetNoUserPreference(int blogId)
        {
            return await Mediator.Send(new GetNoUserPreferenceQuery { BlogId = blogId});
            
        }

        /// PUT: api/RssFeedItems/5
        /// <summary>
        /// Update the user interested flag on an RSS Feed Item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<RssFeedItemDto> Update(int id, UpdateFeedItemCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
