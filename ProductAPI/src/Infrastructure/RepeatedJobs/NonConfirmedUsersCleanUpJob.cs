using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Configuration;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.RepeatedJobs
{
    public class NonConfirmedUsersCleanUpJob : IRepeatedJob
    {
        public int Id { get => 1; }
        private readonly ILogger<NonConfirmedUsersCleanUpJob> _logger;
        private readonly IUserRepository _userRepository;

        public NonConfirmedUsersCleanUpJob(ILogger<NonConfirmedUsersCleanUpJob> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }
        public async Task Run()
        {
            _logger.LogInformation($"Background job with id '{Id}' clears users that are not whitelisted.");
            await _userRepository.ClearUnwhitelistedUsers();
        }
    }
}
