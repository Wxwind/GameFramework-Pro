using Cysharp.Threading.Tasks;
using GameFramework;
using UnityEngine;

namespace GameMain
{
    public class SurvivalGame : GameBase
    {
        private float m_ElapseSeconds;

        public override GameMode GameMode => GameMode.Survival;

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);

            m_ElapseSeconds += elapseSeconds;
            if (m_ElapseSeconds >= 1f)
            {
                m_ElapseSeconds = 0f;
                var tbAsteroid = GameEntry.LubanConfig.Tables.TbAsteroid;
                var randomPositionX = SceneBackground.EnemySpawnBoundary.bounds.min.x +
                                      SceneBackground.EnemySpawnBoundary.bounds.size.x *
                                      (float)Utility.Random.GetRandomDouble();
                var randomPositionZ = SceneBackground.EnemySpawnBoundary.bounds.min.z +
                                      SceneBackground.EnemySpawnBoundary.bounds.size.z *
                                      (float)Utility.Random.GetRandomDouble();
                GameEntry.Entity.ShowAsteroid(new AsteroidData(GameEntry.Entity.GenerateSerialId(),
                    60000 + Utility.Random.GetRandom(tbAsteroid.DataList.Count))
                {
                    Position = new Vector3(randomPositionX, 0f, randomPositionZ)
                }).Forget();
            }
        }
    }
}