using System;

namespace GFPro
{
    public interface IMessageSessionHandler
    {
        void Handle(Session session, object message);
        Type GetMessageType();

        Type GetResponseType();
    }
}