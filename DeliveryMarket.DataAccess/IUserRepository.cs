using System;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model;
using DeliveryMarket.Core.Model.User;
using FluentResults;

namespace DeliveryMarket.DataAccess
{
    public interface IUserRepository
    {
        Task<Result<User>> GetAsync(Guid id);
        Task<Result<User>> CreateAsync(UserTypeEnum type);
    }
}