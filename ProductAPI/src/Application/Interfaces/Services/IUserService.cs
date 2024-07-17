using Application.DTO;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserReadDto> Get(Guid id);
        Task<UserReadDto> Get(string email);
        Task<UserReadDto> Create(UserCreateDto userDto);
        Task<UserReadDto> Update(UserUpdateDto userDto);
        Task<IEnumerable<UserReadDto>> GetAll(QueryParams query);
        Task Delete(IEnumerable<Guid> ids);
        Task<string> AskForPasswordUpdate(Guid userId);
        Task ChangePassword(string email, string newPassword, string confirmationCode);
        Task<OperationResponse> ChangeUsersWhiteListStatus(List<Guid> ids, bool isInWhiteList);
        /// <returns>The value indicating if password is correct.</returns>
        Task<bool> CheckPassword(Guid userId, string password);
    }
}
