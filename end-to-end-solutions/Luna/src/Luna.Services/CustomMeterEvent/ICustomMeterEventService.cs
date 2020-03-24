using Luna.Clients.Models.CustomMetering;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Services.CustomMeterEvent
{
    public interface ICustomMeterEventService
    {
        Task ReportBatchMeterEvents();
    }
}
