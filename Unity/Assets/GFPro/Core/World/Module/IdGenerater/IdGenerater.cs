using System;
using System.Runtime.InteropServices;

namespace GFPro
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IdStruct
    {
        public uint   Time;    // 30bit
        public int    Process; // 18bit
        public ushort Value;   // 16bit

        public long ToLong()
        {
            ulong result = 0;
            result |= Value;
            result |= (ulong)Process << 16;
            result |= (ulong)Time << 34;
            return (long)result;
        }

        public IdStruct(uint time, int process, ushort value)
        {
            Process = process;
            Time = time;
            Value = value;
        }

        public IdStruct(long id)
        {
            var result = (ulong)id;
            Value = (ushort)(result & ushort.MaxValue);
            result >>= 16;
            Process = (int)(result & IdGenerater.Mask18bit);
            result >>= 18;
            Time = (uint)result;
        }

        public override string ToString()
        {
            return $"process: {Process}, time: {Time}, value: {Value}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstanceIdStruct
    {
        public uint Time;    // 当年开始的tick 28bit
        public int  Process; // 18bit
        public uint Value;   // 18bit

        public long ToLong()
        {
            ulong result = 0;
            result |= Value;
            result |= (ulong)Process << 18;
            result |= (ulong)Time << 36;
            return (long)result;
        }

        public InstanceIdStruct(long id)
        {
            var result = (ulong)id;
            Value = (uint)(result & IdGenerater.Mask18bit);
            result >>= 18;
            Process = (int)(result & IdGenerater.Mask18bit);
            result >>= 18;
            Time = (uint)result;
        }

        public InstanceIdStruct(uint time, int process, uint value)
        {
            Time = time;
            Process = process;
            Value = value;
        }

        // 给SceneId使用
        public InstanceIdStruct(int process, uint value)
        {
            Time = 0;
            Process = process;
            Value = value;
        }

        public override string ToString()
        {
            return $"process: {Process}, value: {Value} time: {Time}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UnitIdStruct
    {
        public uint   Time;        // 30bit 34年
        public ushort Zone;        // 10bit 1024个区
        public byte   ProcessMode; // 8bit  Process % 256  一个区最多256个进程
        public ushort Value;       // 16bit 每秒每个进程最大16K个Unit

        public long ToLong()
        {
            ulong result = 0;
            result |= Value;
            result |= (uint)ProcessMode << 16;
            result |= (ulong)Zone << 24;
            result |= (ulong)Time << 34;
            return (long)result;
        }

        public UnitIdStruct(int zone, int process, uint time, ushort value)
        {
            Time = time;
            ProcessMode = (byte)(process % 256);
            Value = value;
            Zone = (ushort)zone;
        }

        public UnitIdStruct(long id)
        {
            var result = (ulong)id;
            Value = (ushort)(result & ushort.MaxValue);
            result >>= 16;
            ProcessMode = (byte)(result & byte.MaxValue);
            result >>= 8;
            Zone = (ushort)(result & 0x03ff);
            result >>= 10;
            Time = (uint)result;
        }

        public override string ToString()
        {
            return $"ProcessMode: {ProcessMode}, value: {Value} time: {Time}";
        }

        public static int GetUnitZone(long unitId)
        {
            var v = (int)((unitId >> 24) & 0x03ff); // 取出10bit
            return v;
        }
    }

    public class IdGenerater : Singleton<IdGenerater>, ISingletonAwake
    {
        public const int Mask18bit = 0x03ffff;

        public const int MaxZone = 1024;

        private long   epoch2020;
        private ushort value;
        private uint   lastIdTime;


        private long epochThisYear;
        private uint instanceIdValue;
        private uint lastInstanceIdTime;


        private ushort unitIdValue;
        private uint   lastUnitIdTime;

        public void Awake()
        {
            var epoch1970tick = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            epoch2020 = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;
            epochThisYear = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;

            lastInstanceIdTime = TimeSinceThisYear();
            if (lastInstanceIdTime <= 0)
            {
                Log.Warning($"lastInstanceIdTime less than 0: {lastInstanceIdTime}");
                lastInstanceIdTime = 1;
            }

            lastIdTime = TimeSince2020();
            if (lastIdTime <= 0)
            {
                Log.Warning($"lastIdTime less than 0: {lastIdTime}");
                lastIdTime = 1;
            }

            lastUnitIdTime = TimeSince2020();
            if (lastUnitIdTime <= 0)
            {
                Log.Warning($"lastUnitIdTime less than 0: {lastUnitIdTime}");
                lastUnitIdTime = 1;
            }
        }

        private uint TimeSince2020()
        {
            var a = (uint)((TimeInfo.Instance.FrameTime - epoch2020) / 1000);
            return a;
        }

        private uint TimeSinceThisYear()
        {
            var a = (uint)((TimeInfo.Instance.FrameTime - epochThisYear) / 1000);
            return a;
        }

        public long GenerateInstanceId()
        {
            var time = TimeSinceThisYear();

            if (time > lastInstanceIdTime)
            {
                lastInstanceIdTime = time;
                instanceIdValue = 0;
            }
            else
            {
                ++instanceIdValue;

                if (instanceIdValue > Mask18bit - 1) // 18bit
                {
                    ++lastInstanceIdTime; // 借用下一秒
                    instanceIdValue = 0;

                    Log.Error($"instanceid count per sec overflow: {time} {lastInstanceIdTime}");
                }
            }

            var instanceIdStruct = new InstanceIdStruct(lastInstanceIdTime, Options.Instance.Process, instanceIdValue);
            return instanceIdStruct.ToLong();
        }

        public long GenerateId()
        {
            var time = TimeSince2020();

            if (time > lastIdTime)
            {
                lastIdTime = time;
                value = 0;
            }
            else
            {
                ++value;

                if (value > ushort.MaxValue - 1)
                {
                    value = 0;
                    ++lastIdTime; // 借用下一秒
                    Log.Error($"id count per sec overflow: {time} {lastIdTime}");
                }
            }

            var idStruct = new IdStruct(lastIdTime, Options.Instance.Process, value);
            return idStruct.ToLong();
        }

        public long GenerateUnitId(int zone)
        {
            if (zone > MaxZone)
            {
                throw new Exception($"zone > MaxZone: {zone}");
            }

            var time = TimeSince2020();

            if (time > lastUnitIdTime)
            {
                lastUnitIdTime = time;
                unitIdValue = 0;
            }
            else
            {
                ++unitIdValue;

                if (unitIdValue > ushort.MaxValue - 1)
                {
                    unitIdValue = 0;
                    ++lastUnitIdTime; // 借用下一秒
                    Log.Error($"unitid count per sec overflow: {time} {lastUnitIdTime}");
                }
            }

            var unitIdStruct = new UnitIdStruct(zone, Options.Instance.Process, lastUnitIdTime, unitIdValue);
            return unitIdStruct.ToLong();
        }
    }
}