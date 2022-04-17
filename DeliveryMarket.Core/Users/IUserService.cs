using System;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.User;
using FluentResults;

namespace DeliveryMarket.Core.Users
{
    public interface IUserService
    {
        Task<Result<User>> GetAsync(Guid id);
        Task<Result<User>> CreateAsync(UserTypeEnum type);
    }
}