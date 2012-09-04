using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    public class HubPipeline : IHubPipeline, IHubPipelineInvoker
    {
        private readonly Stack<IHubPipelineBuilder> _builders = new Stack<IHubPipelineBuilder>();

        private Func<IHubIncomingInvokerContext, Task<object>> _incomingPipeline;
        private Func<IHub, Task> _connectPipeline;
        private Func<IHub, IEnumerable<string>, Task> _reconnectPipeline;
        private Func<IHub, Task> _disconnectPipeline;
        private Func<IHubOutgoingInvokerContext, Task> _outgoingPipeling;

        public IHubPipeline Use(IHubPipelineBuilder builder)
        {
            _builders.Push(builder);
            return this;
        }

        private void EnsurePipeline()
        {
            if (_incomingPipeline == null)
            {
                IHubPipelineBuilder masterBuilder = _builders.Reverse().Aggregate((a, b) => new ComposedBuilder(a, b));
                _incomingPipeline = masterBuilder.BuildIncoming(HubDispatcher.Incoming);
                _connectPipeline = masterBuilder.BuildConnect(HubDispatcher.Connect);
                _reconnectPipeline = masterBuilder.BuildReconnect(HubDispatcher.Reconnect);
                _disconnectPipeline = masterBuilder.BuildDisconnect(HubDispatcher.Disconnect);
                _outgoingPipeling = masterBuilder.BuildOutgoing(HubDispatcher.Outgoing);
            }
        }

        public Task<object> Invoke(IHubIncomingInvokerContext context)
        {
            EnsurePipeline();

            return _incomingPipeline.Invoke(context);
        }

        public Task Connect(IHub hub)
        {
            EnsurePipeline();

            return _connectPipeline.Invoke(hub);
        }

        public Task Reconnect(IHub hub, IEnumerable<string> groups)
        {
            EnsurePipeline();

            return _reconnectPipeline.Invoke(hub, groups);
        }

        public Task Disconnect(IHub hub)
        {
            EnsurePipeline();

            return _disconnectPipeline.Invoke(hub);
        }

        public Task Send(IHubOutgoingInvokerContext context)
        {
            EnsurePipeline();

            return _outgoingPipeling.Invoke(context);
        }

        private class ComposedBuilder : IHubPipelineBuilder
        {
            private readonly IHubPipelineBuilder _left;
            private readonly IHubPipelineBuilder _right;

            public ComposedBuilder(IHubPipelineBuilder left, IHubPipelineBuilder right)
            {
                _left = left;
                _right = right;
            }

            public Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> callback)
            {
                return context =>
                {
                    return _left.BuildIncoming(_right.BuildIncoming(callback))(context);
                };
            }

            public Func<IHub, Task> BuildConnect(Func<IHub, Task> callback)
            {
                return hub =>
                {
                    return _left.BuildConnect(_right.BuildConnect(callback))(hub);
                };
            }

            public Func<IHub, IEnumerable<string>, Task> BuildReconnect(Func<IHub, IEnumerable<string>, Task> callback)
            {
                return (hub, groups) =>
                {
                    return _left.BuildReconnect(_right.BuildReconnect(callback))(hub, groups);
                };
            }

            public Func<IHub, Task> BuildDisconnect(Func<IHub, Task> callback)
            {
                return hub =>
                {
                    return _left.BuildDisconnect(_right.BuildDisconnect(callback))(hub);
                };
            }

            public Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send)
            {
                return context =>
                {
                    return _left.BuildOutgoing(_right.BuildOutgoing(send))(context);
                };
            }
        }
    }
}
