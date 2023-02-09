using Celeste;
using Celeste.Mod.CollabUtils2.UI;
using Celeste.Mod.Entities;
using Celeste.Mod.WorldMapHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;
namespace WorldMapHelper.Entities
{
    [CustomEntity("WorldMapHelper/ExitListener")]
    [Tracked]
    class ExitListener : Entity
    {


        private string BoundExit;
        private string Levelset;
        private string MapID;
        private bool WaitingForWipe;
        private Level lvl;

        public ExitListener(EntityData data, Vector2 offset, EntityID ID) : base(data.Position)
        {
            BoundExit = data.Attr("BoundExit");
        }
        public override void Awake(Scene scene)
        {
            Levelset = (scene as Level).Session.Area.LevelSet.Split('/')[0];
            MapID = (scene as Level).Session.Area.SID.Split('/').Last();
            WorldMapHelperModule.ModuleInstance.EnableHooks(); //TODO: Replace this with a proper fix that checks for an exit listener anywhere in the level upon loading in.
            base.Awake(scene);
            

        }



        public void RedirectScene()
        {
            Engine.Scene = new LevelExitToLobby(LevelExit.Mode.Completed, lvl.Session);
        }

        public override void Update()
        {
            if (WaitingForWipe && !(lvl.Wipe is null))
            {
                lvl.Wipe.OnComplete = this.RedirectScene;
                WaitingForWipe = false;
            }

        }

        public void onLevelEnd(Level level)
        {
            lvl = level;
            WorldMapHelperModule.SaveData.SaveExit(Levelset, MapID, BoundExit);
            WaitingForWipe = true;

        }


    }
}
