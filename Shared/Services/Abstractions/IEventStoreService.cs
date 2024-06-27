using EventStore.Client;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services.Abstractions
{
    public interface IEventStoreService
    {
        Task AppendToStreamAsync(string streamName, ICollection<EventStore.Client.EventData> eventData);
        EventStore.Client.EventData GenerateEventData(object @event);
        Task SubscribeToStreamAsync(string streamName, Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> eventAppeared);
    }
}
