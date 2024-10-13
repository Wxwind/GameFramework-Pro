using System.Collections.Generic;

namespace GFPro
{
    public class CoroutineLockComponent : Singleton<CoroutineLockComponent>, ISingletonAwake, ISingletonUpdate
    {
        private readonly List<CoroutineLockQueueType> list         = new(CoroutineLockType.Max);
        private readonly Queue<(int, long, int)>      nextFrameRun = new();

        public CoroutineLockComponent()
        {
            for (var i = 0; i < CoroutineLockType.Max; ++i)
            {
                var coroutineLockQueueType = new CoroutineLockQueueType(i);
                list.Add(coroutineLockQueueType);
            }
        }

        public void Awake()
        {
        }

        public override void Dispose()
        {
            list.Clear();
            nextFrameRun.Clear();
        }

        public void Update()
        {
            // 循环过程中会有对象继续加入队列
            while (nextFrameRun.Count > 0)
            {
                var (coroutineLockType, key, count) = nextFrameRun.Dequeue();
                Notify(coroutineLockType, key, count);
            }
        }

        public void RunNextCoroutine(int coroutineLockType, long key, int level)
        {
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)
            {
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            }

            nextFrameRun.Enqueue((coroutineLockType, key, level));
        }

        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000)
        {
            var coroutineLockQueueType = list[coroutineLockType];
            return await coroutineLockQueueType.Wait(key, time);
        }

        private void Notify(int coroutineLockType, long key, int level)
        {
            var coroutineLockQueueType = list[coroutineLockType];
            coroutineLockQueueType.Notify(key, level);
        }
    }
}