using Application.DTO;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Common;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserReadDto> Create(UserCreateDto userDto)
        {
            var user = await _userRepository.Create(userDto);
            return user;
        }

        public async Task<UserReadDto> Update(UserUpdateDto userDto)
        {
            var user = await _userRepository.Update(userDto);
            return user;
        }

        public async Task<string> AskForPasswordUpdate(Guid userId)
        {
            var code = await _userRepository.AskForPasswordUpdate(userId);
            return code;
        }

        public async Task ChangePassword(string email, string newPassword, string confirmationCode)
        {
            await _userRepository.ChangePassword(email, newPassword, confirmationCode);
        }

        public Task Delete(IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public async Task<UserReadDto> Get(Guid id)
        {
            var user = await _userRepository.Get(id);
            return user;
        }

        public async Task<UserReadDto> Get(string email)
        {
            var user = await _userRepository.Get(email);
            return user;
        }

        public async Task<IEnumerable<UserReadDto>> GetAll(QueryParams query)
        {
            var users = await _userRepository.GetAll(query);
            return users;
        }

        public async Task<OperationResponse> ChangeUsersWhiteListStatus(List<Guid> ids, bool isInWhiteList)
        {
            OperationResponse result = await _userRepository.ChangeUsersWhiteListStatus(ids, isInWhiteList);
            return result;
        }

        public async Task<bool> CheckPassword(Guid userId, string password)
        {
            bool passed = await _userRepository.CheckPassword(userId, password);
            return passed;
        }
    }
}
