
using Celeste.Mod.CollabUtils2;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using Monocle;
using System;
using System.Reflection;
using WorldMapHelper.Entities;

public enum OverworldState { Ready, Moving, Check, InMenu, Animating, PostAnim }
public enum Direction { Up, Down, Left, Right, None }

public enum PathState { Open, Closed, NewlyOpen }

public enum TeleportType { NotTeleporting, OnLoad, WarpNode }

public enum AnimationState { NotYet, Animating, Done }

namespace Celeste.Mod.WorldMapHelper
{



    public class WorldMapHelperModule : EverestModule
    {

        public static WorldMapHelperModule ModuleInstance;
        public string WarpTarget = "";
        public string ModName = "";
        public TeleportType IsTeleporting = TeleportType.NotTeleporting;
        //public HookManager hookManager;
        public bool HooksActive;
        public Hook JournalHook;

        public Hook HeartsideHook;
        public WorldMapHelperModule()
        {
            ModuleInstance = this;
        }


        public override Type SettingsType => typeof(WorldMapHelperSettings);
        public static WorldMapHelperSettings Settings => (WorldMapHelperSettings)ModuleInstance._Settings;


        public override Type SaveDataType => typeof(WorldMapSaveData);
        public static WorldMapSaveData SaveData => (WorldMapSaveData)ModuleInstance._SaveData;


        //public override Type SessionType => typeof(WorldMapSession);
        //public static WorldMapSession Session => (WorldMapSession)ModuleInstance._Session;



        public override void Load()
        {

        }


        // Optional, initialize anything after Celeste has initialized itself properly.
        public override void Initialize()
        {


        }

        // Optional, do anything requiring either the Celeste or mod content here.
        public override void LoadContent(bool firstLoad)
        {
        }

        public override void Unload()
        {
            DisableHooks();
        }


        public void EnableHooks()
        {
            if (!HooksActive)
            {
                JournalHook = new Hook(typeof(CollabUtils2.Triggers.JournalTrigger).GetMethod("onJournalEnter", BindingFlags.Static | BindingFlags.NonPublic), typeof(WorldMapController).GetMethod("JournalHook"));
                HeartsideHook = new Hook(System.Reflection.Assembly.GetAssembly(typeof(CollabModule)).GetType("Celeste.Mod.CollabUtils2.LobbyHelper").GetMethod("IsHeartSide"), typeof(WorldMapHelperModule).GetMethod("HeartsideHookMethod"));
                On.Celeste.Dialog.Clean += onDialogClean;
                Everest.Events.Level.OnComplete += onLevelEnd;
                Everest.Events.Level.OnExit += onLevelExit;
                HooksActive = true;
            }

        }


        public static bool HeartsideHookMethod(string sid)
        //Collab Utils makes level cards silver if they're goldened unless they're considered a heartside. This simply overrides that behavior and treats everything as a heartside so it's gold.
        {
            return true;
        }

        public void DisableHooks()
        {
            if (HooksActive)
            {

                JournalHook.Dispose();
                HeartsideHook.Dispose();
                On.Celeste.Dialog.Clean -= onDialogClean;
                Everest.Events.Level.OnComplete -= onLevelEnd;
                Everest.Events.Level.OnExit -= onLevelExit;
                HooksActive = false;
            }
        }

        private void onLevelEnd(Level level)
        {
            //pass this off to the ExitListener
            if (!(level.Tracker.GetEntity<ExitListener>() is null))
                level.Tracker.GetEntity<ExitListener>().onLevelEnd((level));

        }

        private void onLevelExit(global::Celeste.Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            //We only want these hooks to work on a screen containing a world map controller, otherwise they should be disabled. 
            //Likewise, hooks are only enabled when a world map controller is active.
            DisableHooks();
        }

        [Command("unlockexit","flags an exit as completed")]
        private static void FlagExit(string MapID, string ExitID)
        {
            SaveData.SaveExit(CollabUtils2.LobbyHelper.GetCollabNameForSID(ModuleInstance.ModName), MapID, ExitID);
        }

        [Command("lockexit", "flags an exit as completed")]
        private static void LockExit(string MapID, string ExitID)
        {
            
            
            SaveData.LockExit(CollabUtils2.LobbyHelper.GetCollabNameForSID(ModuleInstance.ModName), MapID, ExitID);
            
        }


        private string onDialogClean(On.Celeste.Dialog.orig_Clean orig, string name, global::Celeste.Language language)
        {
            //Makes the author string in English.txt optional, as well as 

            if (name == "collabutils2_returntolobby")
                return "Return to Overworld";
            else if (name == "collabutils2_returntolobby_confirm_title")
                return "RETURN TO OVERWORLD?";
            else if (name.EndsWith("_author") && !Dialog.Has(name))
                return "";
            else
                return orig(name, language);
        }

        
        public void Teleport(Level level, String destination)
        {

            if (destination.Contains("|"))
                IsTeleporting = TeleportType.WarpNode;
            else
                IsTeleporting = TeleportType.OnLoad;

            level.OnEndOfFrame += (Action)(() =>
            {
                WarpTarget = destination;
                level.Remove(level.Tracker.GetEntity<Player>());
                level.UnloadLevel();
                level.Session.Level = WarpTarget.Split('|')[0];
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                level.NewLevel = false;
                level.LoadLevel(Player.IntroTypes.Fall);
                IsTeleporting = TeleportType.NotTeleporting;
            });
        }
    }













}

