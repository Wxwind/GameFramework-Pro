using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GFPro
{
    public readonly struct RpcInfo
    {
        public readonly IRequest          Request;
        public readonly ETTask<IResponse> Tcs;

        public RpcInfo(IRequest request)
        {
            Request = request;
            Tcs = ETTask<IResponse>.Create(true);
        }
    }


    public sealed class Session : Entity, IAwake<int>, IDestroy
    {
        public AService AService { get; set; }

        public int ServiceId { get; set; }

        public int RpcId { get; set; }

        public readonly Dictionary<int, RpcInfo> requestCallbacks = new();

        public long LastRecvTime { get; set; }

        public long LastSendTime { get; set; }

        public int Error { get; set; }

        public IPEndPoint RemoteAddress { get; set; }

        public void Awake(int serviceId)
        {
            ServiceId = serviceId;
            var timeNow = TimeInfo.Instance.ClientNow();
            LastRecvTime = timeNow;
            LastSendTime = timeNow;

            requestCallbacks.Clear();

            Log.Info($"session create: zone: {this.DomainZone()} id: {Id} {timeNow} ");
        }

        public void Destroy()
        {
            NetServices.Instance.RemoveChannel(ServiceId, Id, Error);

            foreach (var responseCallback in requestCallbacks.Values.ToArray())
            {
                responseCallback.Tcs.SetException(new RpcException(Error, $"session dispose: {Id} {RemoteAddress}"));
            }

            Log.Info($"session dispose: {RemoteAddress} id: {Id} ErrorCode: {Error}, please see ErrorCode.cs! {TimeInfo.Instance.ClientNow()}");

            requestCallbacks.Clear();
        }

        public void OnResponse(IResponse response)
        {
            if (!requestCallbacks.TryGetValue(response.RpcId, out var action))
            {
                return;
            }

            requestCallbacks.Remove(response.RpcId);
            if (ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                action.Tcs.SetException(new Exception($"Rpc error, request: {action.Request} response: {response}"));
                return;
            }

            action.Tcs.SetResult(response);
        }

        public async ETTask<IResponse> Call(IRequest request, ETCancellationToken cancellationToken)
        {
            var rpcId = ++Session.RpcId;
            var rpcInfo = new RpcInfo(request);
            requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;

            Send(request);

            void CancelAction()
            {
                if (!requestCallbacks.TryGetValue(rpcId, out var action))
                {
                    return;
                }

                requestCallbacks.Remove(rpcId);
                var responseType = OpcodeTypeComponent.Instance.GetResponseType(action.Request.GetType());
                var response = (IResponse)Activator.CreateInstance(responseType);
                response.Error = ErrorCore.ERR_Cancel;
                action.Tcs.SetResult(response);
            }

            IResponse ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await rpcInfo.Tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            return ret;
        }

        public async ETTask<IResponse> Call(IRequest request)
        {
            var rpcId = ++RpcId;
            var rpcInfo = new RpcInfo(request);
            requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;
            Send(request);
            return await rpcInfo.Tcs;
        }

        public void Send(IMessage message)
        {
            Send(0, message);
        }

        public void Send(long actorId, IMessage message)
        {
            LastSendTime = TimeInfo.Instance.ClientNow();
            OpcodeHelper.LogMsg(this.DomainZone(), message);
            var (opcode, memoryBuffer) = MessageSerializeHelper.ToMemoryBuffer(AService, actorId, message);
            AService.Send(Id, memoryBuffer);
        }
    }
}