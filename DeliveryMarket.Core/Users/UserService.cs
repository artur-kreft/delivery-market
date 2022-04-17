using System;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.User;
using DeliveryMarket.DataAccess;
using FluentResults;

namespace DeliveryMarket.Core.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>Shipper or Carrier</returns>
        public Task<Result<User>> GetAsync(Guid id)
        {
            return _userRepository.GetAsync(id);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="type">type of user to create</param>
        /// <returns>created user</returns>
        public Task<Result<User>> CreateAsync(UserTypeEnum type)
        {
            return _userRepository.CreateAsync(type);
        }
    }
}