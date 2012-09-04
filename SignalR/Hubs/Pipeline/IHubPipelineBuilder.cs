using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHubPipelineBuilder
    {
        Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke);
        Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send);

        Func<IHub, Task> BuildConnect(Func<IHub, Task> connect);
        Func<IHub, IEnumerable<string>, Task> BuildReconnect(Func<IHub, IEnumerable<string>, Task> reconnect);
        Func<IHub, Task> BuildDisconnect(Func<IHub, Task> disconnect);
    }
}
