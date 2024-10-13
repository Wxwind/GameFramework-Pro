using System;

namespace GFPro.Procedure
{
    /// <summary>
    /// 流程组件。
    /// </summary>
    public sealed class ProcedureComponent : Entity, IDestroy, IAwake
    {
        private ProcedureBase m_EntranceProcedure;

        private string[] m_AvailableProcedureTypeNames;

        private string m_EntranceProcedureTypeName;

        private IFsmComponent            m_FsmComponent;
        private IFsm<ProcedureComponent> m_ProcedureFsm;

        /// <summary>
        /// 获取当前流程。
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (m_ProcedureFsm == null)
                {
                    throw new GameFrameworkException("You must initialize procedure first.");
                }

                return (ProcedureBase)m_ProcedureFsm.CurrentState;
            }
        }

        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        public float CurrentProcedureTime
        {
            get
            {
                if (m_ProcedureFsm == null)
                {
                    throw new GameFrameworkException("You must initialize procedure first.");
                }

                return m_ProcedureFsm.CurrentStateTime;
            }
        }

        public void Awake()
        {
            var procedures = new ProcedureBase[m_AvailableProcedureTypeNames.Length];
            for (var i = 0; i < m_AvailableProcedureTypeNames.Length; i++)
            {
                var procedureType = Utility.Assembly.GetType(m_AvailableProcedureTypeNames[i]);
                if (procedureType == null)
                {
                    Log.Error($"Can not find procedure type '{m_AvailableProcedureTypeNames[i]}'.");
                    return;
                }

                procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
                if (procedures[i] == null)
                {
                    Log.Error($"Can not create procedure instance '{m_AvailableProcedureTypeNames[i]}'.");
                    return;
                }

                if (m_EntranceProcedureTypeName == m_AvailableProcedureTypeNames[i])
                    m_EntranceProcedure = procedures[i];
            }

            if (m_EntranceProcedure == null)
            {
                Log.Error("Entrance procedure is invalid.");
                return;
            }


            var fsmManager = FsmComponent.Instance;
            if (fsmManager == null)
            {
                throw new GameFrameworkException("FSM manager is invalid.");
            }

            m_FsmComponent = fsmManager;
            m_ProcedureFsm = m_FsmComponent.CreateFsm(this, procedures);

            // 从原本等待一帧启动变为直接启动
            m_ProcedureFsm.Start(m_EntranceProcedure.GetType());
        }

        public void Destroy()
        {
            if (m_FsmComponent != null)
            {
                if (m_ProcedureFsm != null)
                {
                    m_FsmComponent.DestroyFsm(m_ProcedureFsm);
                    m_ProcedureFsm = null;
                }

                m_FsmComponent = null;
            }
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <typeparam name="T">要检查的流程类型。</typeparam>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure<T>() where T : ProcedureBase
        {
            return m_ProcedureFsm.HasState<T>();
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <param name="procedureType">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure(Type procedureType)
        {
            return m_ProcedureFsm.HasState(procedureType);
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            return m_ProcedureFsm.GetState<T>();
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure(Type procedureType)
        {
            return (ProcedureBase)m_ProcedureFsm.GetState(procedureType);
        }
    }
}