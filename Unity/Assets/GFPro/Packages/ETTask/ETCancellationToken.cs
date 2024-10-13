using System;
using System.Collections.Generic;

namespace GFPro
{
    public class ETCancellationToken
    {
        private HashSet<Action> actions = new();

        public void Add(Action callback)
        {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
            actions.Add(callback);
        }

        public void Remove(Action callback)
        {
            actions?.Remove(callback);
        }

        public bool IsDispose()
        {
            return actions == null;
        }

        public void Cancel()
        {
            if (actions == null)
            {
                return;
            }

            Invoke();
        }

        private void Invoke()
        {
            var runActions = actions;
            actions = null;
            try
            {
                foreach (var action in runActions)
                {
                    action.Invoke();
                }
            }
            catch (Exception e)
            {
                ETTask.ExceptionHandler.Invoke(e);
            }
        }
    }
}