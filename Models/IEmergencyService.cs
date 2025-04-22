using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Task3_2.Models
{
    public interface IEmergencyService
    {
        Task RespondToAccident(double x, double y);
        bool IsBusy { get; }
        event EventHandler<Car> EmergencyVehicleArrived;
    }
}