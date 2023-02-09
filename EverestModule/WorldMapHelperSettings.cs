namespace Celeste.Mod.WorldMapHelper
{
    public class WorldMapHelperSettings : EverestModuleSettings
    {
        public enum MapMovementType { Normal, Fast, Absurd }

        private bool resetMapPositionOnNextRestart;

        public const float MoveSpeed_N = 1;
        public const float MoveSpeed_F = 4;
        public const float MoveSpeed_A = 8;


        public MapMovementType MapMovementSpeed { get; set; }
        public bool ReturnToStartingPointNextRestart { get => resetMapPositionOnNextRestart; set => resetMapPositionOnNextRestart = value; }

        public float GetMapMovementSpeed()
        {
            switch (MapMovementSpeed)
            {
                case MapMovementType.Normal:
                    return MoveSpeed_N;
                case MapMovementType.Fast:
                    return MoveSpeed_F;
                case MapMovementType.Absurd:
                    return MoveSpeed_A;
            }
            return 1;
        }

    }
}
