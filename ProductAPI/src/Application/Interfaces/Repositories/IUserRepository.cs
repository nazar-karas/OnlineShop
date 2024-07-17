using Application.DTO;
using Domain.Common;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository
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
        void ClearConfirmationCodes();
        Task ClearUnwhitelistedUsers();
        /// <returns>The value indicating if password is correct.</returns>
        Task<bool> CheckPassword(Guid userId, string password);
    }
}
