using System;
using System.Diagnostics;
using FluentResults;

namespace DeliveryMarket.Notification
{
    public interface INotifyService
    {
        Result Notify(Guid userId, string title, string message);
    }
}