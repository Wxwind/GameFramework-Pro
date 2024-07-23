using System.Runtime.InteropServices;

namespace GameMain
{
    [StructLayout(LayoutKind.Auto)]
    public struct ImpactData
    {
        public ImpactData(CampType camp, int hp, int attack, int defense)
        {
            Camp = camp;
            HP = hp;
            Attack = attack;
            Defense = defense;
        }

        public CampType Camp { get; }

        public int HP { get; }

        public int Attack { get; }

        public int Defense { get; }
    }
}