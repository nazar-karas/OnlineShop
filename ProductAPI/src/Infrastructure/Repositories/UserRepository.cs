using Application.DTO;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Exceptions;
using Hangfire;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task<UserReadDto> Create(UserCreateDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userDto.Email);

            if (user != null)
            {
                throw new AlreadyExistsException($"The user with email '{userDto.Email}' already exists");
            }

            List<string> roles = _configuration.GetSection("Security:JwtSettings:Roles").Get<List<string>>();

            if (!roles.Contains(userDto.Role.ToLower()))
            {
                throw new ArgumentException($"You passed a role '{userDto.Role}', which is invalid.");
            }

            string salt = UserHelper.CreateRandomSalt();
            string hash = UserHelper.ComputeSaltedHash(userDto.Password, salt);

            var dataObject = new User()
            {
                Id = Guid.NewGuid(),
                Email = userDto.Email,
                Name = userDto.Name,
                Surname = userDto.Surname,
                Role = userDto.Role.ToLower(),
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await _context.Users.AddAsync(dataObject);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<UserReadDto>(dataObject);
            return resultDto;
        }

        public async Task<UserReadDto> Update(UserUpdateDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userDto.Id);

            if (user == null)
            {
                throw new NotFoundException($"The user with email '{userDto.Email}' not found.");
            }

            bool isEmailUsed = await _context.Users.AnyAsync(x => x.Email == userDto.Email);

            if (user.Email != userDto.Email && isEmailUsed)
            {
                throw new AlreadyExistsException($"The email '{userDto.Email}' is already used.");
            }

            if (user.Role != userDto.Role.ToLower())
            {
                List<string> roles = _configuration.GetSection("Security:JwtSettings:Roles").Get<List<string>>();

                if (!roles.Contains(userDto.Role.ToLower()))
                {
                    throw new ArgumentException($"You passed a role '{userDto.Role}', which is invalid.");
                }

                user.Role = userDto.Role;
                user.IsInWhiteList = false;
            }
            user.Email = userDto.Email;
            user.Name = userDto.Name;
            user.Surname = userDto.Surname;
            
            _context.Update(user);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<UserReadDto>(user);
            return resultDto;
        }

        public Task Delete(IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public async Task<string> AskForPasswordUpdate(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            
            if (user == null)
            {
                throw new NotFoundException($"The user with id '{userId}' not found.");
            }

            string code = UserHelper.GenerateConfirmationCode();
            
            // ensure we have a unique confirmation code
            while (await _context.Users.AnyAsync(x => x.ConfirmationCode == code))
            {
                code = UserHelper.GenerateConfirmationCode();
            }

            user.ConfirmationCode = code;

            _context.Update(user);
            await _context.SaveChangesAsync();
            
            TimeSpan delay = _configuration.GetSection("Security:CodeLifetime").Get<TimeSpan>();
            BackgroundJob.Schedule(() => ClearConfirmationCodes(), delay);

            return code;
        }

        public async Task ChangePassword(string email, string newPassword, string confirmationCode)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                throw new NotFoundException($"The user with email '{email}' not found.");
            }

            if (user.ConfirmationCode != confirmationCode)
            {
                throw new ArgumentException($"The confirmation code '{confirmationCode}' is invalid or too old");
            }

            string salt = UserHelper.CreateRandomSalt();
            string hash = UserHelper.ComputeSaltedHash(newPassword, salt);

            user.PasswordSalt = salt;
            user.PasswordHash = hash;
            user.ConfirmationCode = null;

            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> CheckPassword(Guid userId, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new NotFoundException($"The user with id '{userId}' not found.");
            }

            string hashToCheck = UserHelper.ComputeSaltedHash(password, user.PasswordSalt);

            return string.Equals(hashToCheck, user.PasswordHash);
        }

        public async Task<UserReadDto> Get(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            var dto = _mapper.Map<UserReadDto>(user);
            return dto;
        }

        public async Task<UserReadDto> Get(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            var dto = _mapper.Map<UserReadDto>(user);
            return dto;
        }

        public async Task<IEnumerable<UserReadDto>> GetAll(QueryParams query)
        {
            var dos = await _context.Users.ToListAsync();

            if (string.Equals(query.SortBy, "Role", StringComparison.Ordinal))
            {
                dos = dos.OrderBy(x => x.Role).ToList();
            }
            else if (string.Equals(query.SortBy, "IsInWhiteList", StringComparison.Ordinal))
            {
                dos = dos.OrderBy(x => x.IsInWhiteList).ToList();
            }
            else
            {
                dos = dos.OrderBy(x => x.Email).ToList();
            }

            if (query.FilterBy != null)
            {
                if (!string.IsNullOrEmpty(query.FilterBy.Name))
                {
                    dos = dos.Where(x => x.Name == query.FilterBy.Name).ToList();
                }

                if (!string.IsNullOrEmpty(query.FilterBy.Surname))
                {
                    dos = dos.Where(x => x.Name == query.FilterBy.Surname).ToList();
                }

                if (!string.IsNullOrEmpty(query.FilterBy.Role))
                {
                    dos = dos.Where(x => x.Name == query.FilterBy.Role).ToList();
                }

                if (query.FilterBy.IsInWhiteList.HasValue)
                {
                    dos = dos.Where(x => x.IsInWhiteList == query.FilterBy.IsInWhiteList).ToList();
                }
            }

            var dtos = _mapper.Map<IEnumerable<UserReadDto>>(dos);
            return dtos;
        }

        public async Task<OperationResponse> ChangeUsersWhiteListStatus(List<Guid> ids, bool isInWhiteList)
        {
            var response = new OperationResponse();
            response.Entities = new List<ProcessedEntitiy>();

            var users = await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

            foreach (var id in ids)
            {
                if (users.Any(u => u.Id == id))
                {
                    response.Entities.Add(new ProcessedEntitiy()
                    {
                        Name = id.ToString(),
                        ErrorMessage = null
                    });
                }
                else
                {
                    response.Entities.Add(new ProcessedEntitiy()
                    {
                        Name = id.ToString(),
                        ErrorMessage = $"The user with id '{id}' not found."
                    });
                }
            }

            if (users.Count == 0)
            {
                return response;
            }

            foreach (var user in users)
            {
                user.IsInWhiteList = isInWhiteList;
            }

            _context.UpdateRange(users);
            await _context.SaveChangesAsync();

            return response;
        }

        public void ClearConfirmationCodes()
        {
            var users = _context.Users.ToList();

            users.ForEach(u =>
            {
                u.ConfirmationCode = null;
            });

            _context.UpdateRange(users);
            _context.SaveChanges();
        }

        public async Task ClearUnwhitelistedUsers()
        {
            var users = _context.Users.ToList();

            var usersToDelete = users.Where(u => !u.IsInWhiteList).ToList();

            _context.RemoveRange(usersToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
