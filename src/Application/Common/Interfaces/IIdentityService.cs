﻿using TechRSSReader.Application.Common.Models;
using System.Threading.Tasks;

namespace TechRSSReader.Application
{
    public interface IIdentityService
    {
        Task<string> GetUserNameAsync(string userId);

        Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

        Task<Result> DeleteUserAsync(string userId);
    }
}
