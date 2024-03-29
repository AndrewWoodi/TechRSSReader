﻿using TechRSSReader.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace TechRSSReader.Application.Common.Behaviours
{
    public class RequestLogger<TRequest> : IRequestPreProcessor<TRequest>
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public RequestLogger(ILogger<TRequest> logger, ICurrentUserService currentUserService, IIdentityService identityService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                _logger.LogDebug("TechRSSReader Request: {Name} {@Request}",
                    requestName, request);
            }
            else
            {
                var userName = await _identityService.GetUserNameAsync(userId);
                _logger.LogDebug("TechRSSReader Request: {Name} {@UserId} {@UserName} {@Request}",
                    requestName, userId, userName, request);
            }

        }
    }
}
