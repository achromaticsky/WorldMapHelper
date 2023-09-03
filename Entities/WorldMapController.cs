using Celeste;
using Celeste.Mod.CollabUtils2.UI;
using Celeste.Mod.Entities;
using Celeste.Mod.WorldMapHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;


namespace WorldMapHelper.Entities
{
    [CustomEntity("WorldMapHelper/WorldMapController")]
    [Tracked]

    class WorldMapController : Entity

    {
        //Map Matrix Values
        //0 -- Impassable (no path or locked path)
        //-1 -- Path

        //1+ -- Level Node

        public static WorldMapController WorldMapControllerInstance;
        private Vector2 WorldMapOrigin;
        private int[,] WorldMapMatrix;
        private Vector2 MapPosition;
        private Vector2 MapDestination;
        private Direction MapDirection;
        private Scene _scene;
        private OverworldState MapCursorState;
        private List<Vector2> Route;
        private Vector2 MoveSpeed;
        private float PathClock;
        private bool NewInput;
        private bool ReleasedConfirmButton = false;
        private bool PlayerIsBadeline;
        private int NextNodeID = 1;
        private int CurrentPriorityCount;
        private string Levelset;
        private string SIDPrefix;
        private string CurrentRoom;
        private LevelNameDisplay LevelDisplay;
        private Sprite WorldMapSprite;
        private List<WorldMapPath> AnimationQueue = new List<WorldMapPath>();
        private bool _JustTeleported = false;
        private float AnimationClock = 90;
        private float _pathLerpValue;
        private bool _pathLerpBack;
        private Color _pathCycleA;
        private Color _pathCycleB;
        private Color _pathCurrentColor;
        private Vector2 _LastAnimationTarget;
        public bool ArrivingFromWarpNode = false;
        public bool JustTeleported { get => _JustTeleported; set => _JustTeleported = value; }
        public Color PathCurrentColor { get => _pathCurrentColor; set => _pathCurrentColor = value; }

        public WorldMapController(Vector2 position) : base(position)
        {
            PlayerIsBadeline = SaveData.Instance.Assists.PlayAsBadeline;
            InitializeSprite(SaveData.Instance.Assists.PlayAsBadeline);
            WorldMapControllerInstance = this;
            WorldMapHelperModule.ModuleInstance.EnableHooks();
        }

        public static void Load()
        {


        }

        public void OnExit(global::Celeste.Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
        }


        public override void Awake(Scene scene)
        {
            _scene = scene;
            scene.Tracker.GetEntity<Player>().Position = this.Position;
            Levelset = (_scene as Level).Session.Area.LevelSet.Replace("/0-Lobbies", "");

            LevelDisplay = new LevelNameDisplay(Vector2.Zero);
            _scene.Add(LevelDisplay);

            SIDPrefix = (_scene as Level).Session.Area.SID.Replace("/0-Lobbies", "") + "/";
            CurrentRoom = (_scene as Level).Session.Level;
            scene.Tracker.GetEntity<Player>().StateMachine.ForceState(Player.StFrozen);
            scene.Tracker.GetEntity<Player>().Visible = false;
            _scene.Tracker.GetEntity<Player>().Active = false;
            (_scene as Level).CanRetry = false;

            WorldMapHelperModule.ModuleInstance.ModName = Celeste.AreaData.Get(scene).Mode[0].Path;

            if (WorldMapHelperModule.Settings.ReturnToStartingPointNextRestart)
            {
                WorldMapHelperModule.SaveData.ClearCoords(Levelset);
                WorldMapHelperModule.Settings.ReturnToStartingPointNextRestart = false;
            }
            
            if (WorldMapHelperModule.ModuleInstance.IsTeleporting == TeleportType.NotTeleporting)
            {
                if (WorldMapHelperModule.SaveData.GetCurrentRoom(Levelset) != "")
                {
                    WorldMapHelperModule.ModuleInstance.Teleport((_scene as Level), WorldMapHelperModule.SaveData.GetCurrentRoom(Levelset));
                    return;
                }
                else
                {
                    WorldMapHelperModule.ModuleInstance.Teleport((_scene as Level), CurrentRoom);
                    return;
                }
            }


            InitializeMapMatrix(scene.Tracker.GetEntities<WorldMapPath>(), scene.Tracker.GetEntities<WorldMapNode>());



            if (WorldMapHelperModule.SaveData.CheckCoords(Levelset, CurrentRoom) && WorldMapHelperModule.ModuleInstance.IsTeleporting == TeleportType.OnLoad)
            {
                MapPosition = WorldMapHelperModule.SaveData.GetCoords(Levelset);
                LevelDisplay.Text1 = GetCurrentNodeText();
            }

            if (WorldMapHelperModule.ModuleInstance.IsTeleporting == TeleportType.WarpNode)
            {
                MoveToWarpDestination(WorldMapHelperModule.ModuleInstance.WarpTarget.Split('|')[1]);
                LevelDisplay.Text1 = GetCurrentNodeText();
                WorldMapHelperModule.SaveData.SaveCoords(Levelset, CurrentRoom, MapPosition);
            }


            Position = MatrixToGame(MapPosition);

            MapDestination = MapPosition;



            if (AnimationQueue.Count > 0)
            {
                foreach (WorldMapNode n in (_scene as Level).Tracker.GetEntities<WorldMapNode>())
                    InitializeNode(n, true);
                MapCursorState = OverworldState.Animating;
            }

            (_scene as Level).Camera.CenterOrigin();
            (_scene as Level).Camera.Position = CameraSafePosition(this.Position);
            WorldMapSprite.Play("neutral");
            _LastAnimationTarget = CameraSafePosition(this.Position);
            _pathCycleA = new Color(255, 255, 90);
            _pathCycleB = new Color(255, 255, 200);
            _pathLerpValue = 0;
            _pathCurrentColor = Color.Lerp(_pathCycleA, _pathCycleB, 0);

            new DynData<Overworld>().Set("WorldMapHelperForcedArea", AreaData.Get((_scene as Level).Session.Area.SID));

            base.Awake(scene);

        }

        public override void Render()
        {
            this.Depth = -99999;




            //placeholder.Draw(this.Position);
            WorldMapSprite.Position = this.Position + new Vector2(-12, -20);

            WorldMapSprite.Render();



            base.Render();

        }

        public override void Update()
        {
            //slightly kludgey solution to CollabUtils unfreezing the player at an unknown time
            if (_scene.Tracker.CountEntities<Player>() > 0)
            {
                if (_scene.Tracker.GetEntity<Player>().StateMachine.State != Player.StFrozen)
                {
                    _scene.Tracker.GetEntity<Player>().StateMachine.ForceState(Player.StFrozen);
                    _scene.Tracker.GetEntity<Player>().Active = false;
                }
            }

            //if the menu was just closed, return control
            if (!InGameOverworldHelper.IsOpen && MapCursorState == OverworldState.InMenu)
            {
                MapCursorState = OverworldState.Ready;
                LevelDisplay.Visible = true;
            }

            if (MapCursorState == OverworldState.Animating)
            {
                (_scene as Level).Camera.Approach(AnimationTarget(), 0.25f);
            }
            else
            {
                (_scene as Level).Camera.Approach(CameraSafePosition(this.Position), 0.125f);
            }



            base.Update();
            WorldMapSprite.Update();

            _pathLerpValue += 0.02f * (float)(_pathLerpBack ? -1 : 1);
            if (_pathLerpValue >= 1)
            {
                _pathLerpValue = 1;
                _pathLerpBack = true;
            }
            if (_pathLerpValue <= 0)
            {
                _pathLerpValue = 0;
                _pathLerpBack = false;
            }
            _pathCurrentColor = Color.Lerp(_pathCycleA, _pathCycleB, _pathLerpValue) * 0.7f;


            //Ready to accept input
            if (MapCursorState == OverworldState.Ready)
            {

                if (Input.MoveY.Value < 0 && Input.MoveY.PreviousValue >= 0) //Up
                {
                    MapDirection = Direction.Up;
                    NewInput = true;

                }
                if (Input.MoveY.Value > 0 && Input.MoveY.PreviousValue <= 0) //Down
                {
                    MapDirection = Direction.Down;
                    NewInput = true;

                }
                if (Input.MoveX.Value < 0 && Input.MoveX.PreviousValue >= 0) //Left
                {
                    MapDirection = Direction.Left;
                    NewInput = true;

                }
                if (Input.MoveX.Value > 0 && Input.MoveX.PreviousValue <= 0) //Right
                {
                    MapDirection = Direction.Right;
                    NewInput = true;


                }
                if (NewInput)
                {
                    MapCursorState = OverworldState.Check;
                    PlanRoute();
                    NewInput = false;
                }

                if (Input.Dash.Pressed || Input.Jump.Pressed || Input.MenuConfirm.Pressed)
                {
                    if (ReleasedConfirmButton) //prevents unintended node activation from starting the map on a node with the button held.
                    {
                        MapCursorState = OverworldState.InMenu;
                        OpenSelectedLevel();
                    }

                }
                else
                {
                    ReleasedConfirmButton = true;
                }

                if (Input.MenuJournal.Pressed)
                {
                    OpenJournal();
                }



            }

            //Moving on a path, but just reached a new space on the overworld.
            if (MapCursorState == OverworldState.Check)
            {
                if (Route.Count == 0)
                {
                    MapCursorState = OverworldState.Ready;
                    Position = MatrixToGame(MapPosition);
                    LevelDisplay.Text1 = GetCurrentNodeText();
                    WorldMapHelperModule.SaveData.SaveCoords(Levelset, CurrentRoom, MapPosition);
                    WorldMapSprite.Play("neutral");
                }
                else
                {

                    if (Route.Count == 1 || Route.First() != Route.ElementAt<Vector2>(1))
                        Position = MatrixToGame(MapPosition);

                    MapPosition.X += Route.First().X;
                    MapPosition.Y += Route.First().Y;

                    if (Route.First().Y == -1)
                        WorldMapSprite.Play("up");

                    if (Route.First().Y == 1)
                        WorldMapSprite.Play("down");

                    if (Route.First().X == 1)
                        WorldMapSprite.Play("right");

                    if (Route.First().X == -1)
                        WorldMapSprite.Play("left");

                    MoveSpeed = Route.First() * (int)WorldMapHelperModule.Settings.GetMapMovementSpeed();
                    Route.RemoveAt(0);
                    MapCursorState = OverworldState.Moving;
                    PathClock = 8;

                }
            }

            //Actively moving between path tiles.
            if (MapCursorState == OverworldState.Moving)
            {
                Position += MoveSpeed;
                PathClock -= MoveSpeed.Length();
                if (PathClock <= 0)
                    MapCursorState = OverworldState.Check;
            }

            //Path opening animation is active
            if (MapCursorState == OverworldState.Animating)
            {
                RunMapAnimations();
            }

            if (SaveData.Instance.Assists.PlayAsBadeline != PlayerIsBadeline)
            {
                PlayerIsBadeline = SaveData.Instance.Assists.PlayAsBadeline;
                InitializeSprite(PlayerIsBadeline);
                WorldMapSprite.Play("neutral");
            }



        }

        private void PlanRoute()
        {
            Route = new List<Vector2>();
            bool FirstMove = true;
            //First Move
            MapDestination = MapPosition;

            if (CheckDirection(MapPosition, MapDirection) != 0)
            {
                ShiftDestination(MapDirection);

                Direction PlannedDirection = MapDirection;
                //Subsequent Moves
                int ValidMoves = 1;
                while (ValidMoves == 1)
                {
                    ValidMoves = 0;
                    //only one facing direction other than your last move is active

                    //check up
                    if (CheckDirection(MapDestination, Direction.Up) != 0 && MapDirection != Direction.Down)
                    {
                        ValidMoves++;
                        PlannedDirection = Direction.Up;
                    }

                    if (CheckDirection(MapDestination, Direction.Down) != 0 && MapDirection != Direction.Up)
                    {
                        ValidMoves++;
                        PlannedDirection = Direction.Down;
                    }

                    if (CheckDirection(MapDestination, Direction.Left) != 0 && MapDirection != Direction.Right)
                    {
                        ValidMoves++;
                        PlannedDirection = Direction.Left;
                    }

                    if (CheckDirection(MapDestination, Direction.Right) != 0 && MapDirection != Direction.Left)
                    {
                        ValidMoves++;
                        PlannedDirection = Direction.Right;
                    }

                    if (ValidMoves == 1 && (WorldMapMatrix[(int)MapDestination.X, (int)MapDestination.Y] == -1 || FirstMove))
                    {
                        ShiftDestination(PlannedDirection);
                        FirstMove = false;
                        //System.Console.WriteLine("Moved to " + MapDestination.ToString());
                        //VisualizeMatrix();

                    }

                    if (ValidMoves == 1 && (WorldMapMatrix[(int)MapDestination.X, (int)MapDestination.Y] != -1 && !FirstMove))
                    {
                        ValidMoves = 0;


                    }
                }



            }


        }

        private string GetCurrentNodeText()
        {
            try
            {
                foreach (WorldMapNode n in _scene.Tracker.GetEntities<WorldMapNode>())
                {
                    if (n.NodeID == WorldMapMatrix[(int)MapPosition.X, (int)MapPosition.Y])
                    {
                        return n.NodeText;
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                return "Error displaying node text."; //Catch for a crash of unknown cause discovered by Monika. 
            }
        }

        private void InitializeMapMatrix(List<Entity> pathList, List<Entity> nodeList)
        {

            //set bounds
            float minX = 99999, minY = 99999, maxX = 0, maxY = 0;

            foreach (WorldMapPath p in pathList)
            {
                if (p.Position.X < minX)
                    minX = p.Position.X;
                if (p.Position.Y < minY)
                    minY = p.Position.Y;
                if (p.Position.X + p.Width > maxX)
                    maxX = p.Position.X + p.Width;
                if (p.Position.Y + p.Height > maxY)
                    maxY = p.Position.Y + p.Height;
            }


            foreach (WorldMapNode n in nodeList)
            {
                if (n.Position.X < minX)
                    minX = n.Position.X;
                if (n.Position.Y < minY)
                    minY = n.Position.Y;
                if (n.Position.X + n.Width > maxX)
                    maxX = n.Position.X + 24;
                if (n.Position.Y + n.Height > maxY)
                    maxY = n.Position.Y + 24;
            }

            //create a margin of 2 tiles around each side of the map to allow safe checking
            minX -= 32;
            minY -= 32;
            maxX += 32;
            maxY += 32;

            WorldMapOrigin = new Vector2(minX, minY);
            WorldMapMatrix = new int[(int)((maxX - minX) / 8), (int)((maxY - minY) / 8)];

            foreach (WorldMapPath p in pathList)
            {
                p.Levelset = (_scene as Level).Session.Area.GetLevelSet().Split('/')[0];

                if (p.CheckOpen() == PathState.Open || Celeste.SaveData.Instance.CheatMode)
                {
                    FillMatrixChunk(-1, (int)GameToMatrix(p.TopLeft).X, (int)GameToMatrix(p.TopLeft).Y, (int)GameToMatrix(p.BottomRight).X, (int)GameToMatrix(p.BottomRight).Y);
                    //p.Visible = true;
                }

                if (p.CheckOpen() == PathState.NewlyOpen)
                {
                    AnimationQueue.Add(p);
                    //p.Visible = true;
                }

            }

            foreach (WorldMapNode n in nodeList)
            {
                InitializeNode(n); //Initialize nodes with only the already-active paths visible.
            }

            foreach (WorldMapPath p in AnimationQueue) //Now add the newly active paths to the matrix
                FillMatrixChunk(-1, (int)GameToMatrix(p.TopLeft).X, (int)GameToMatrix(p.TopLeft).Y, (int)GameToMatrix(p.BottomRight).X, (int)GameToMatrix(p.BottomRight).Y);


            if (AnimationQueue.Count > 0)
            {
                AnimationQueue.Sort();
                CurrentPriorityCount = AnimationQueue.First().Priority;
            }
            this.Position = MatrixToGame(MapPosition);
        }

        private Vector2 MatrixToGame(Vector2 MatrixCoords)
        {
            return MatrixCoords * 8 + WorldMapOrigin; //+ new Vector2(4,4);
        }

        private Vector2 GameToMatrix(Vector2 GameCoords)
        {
            return (GameCoords - WorldMapOrigin) / 8;
        }

        public void FillMatrixChunk(int Value, int StartX, int StartY, int EndX, int EndY)
        {
            for (int i = StartX; i < EndX; i++)
            {
                for (int j = StartY; j < EndY; j++)
                {
                    WorldMapMatrix[i, j] = Value;

                }

            }
        }

        private Vector2 CameraSafePosition(Vector2 pos)

        {

            Vector2 SafePos = pos;

            if (SafePos.X + 160 > (_scene as Level).Bounds.Right)
                SafePos.X = (_scene as Level).Bounds.Right - 160;

            if (SafePos.X - 160 < (_scene as Level).Bounds.Left)
                SafePos.X = (_scene as Level).Bounds.Left + 160;

            if (SafePos.Y + 90 > (_scene as Level).Bounds.Bottom)
                SafePos.Y = (_scene as Level).Bounds.Bottom - 90;

            if (SafePos.Y - 90 < (_scene as Level).Bounds.Top)
                SafePos.Y = (_scene as Level).Bounds.Top + 90;


            return SafePos;
        }

        private void InitializeNode(WorldMapNode n, bool reinitializing = false)

        {
            bool WasVisible = n.Visible;
            Vector2 nodeMapPosition = GameToMatrix(n.Position);
            nodeMapPosition.X++;
            nodeMapPosition.Y++;
            if (!reinitializing)
            {
                n.Visible = false;
                n.NodeID = NextNodeID;
                NextNodeID++;

                //logical node position is in the center of the node, +1,+1 from its physical position on the grid


                WorldMapMatrix[(int)nodeMapPosition.X, (int)nodeMapPosition.Y] = n.NodeID;

                if (n.StartingPoint)
                {
                    MapPosition = nodeMapPosition;
                    n.Visible = true;
                    LevelDisplay.Text1 = GetCurrentNodeText();
                }

            }
            //check all 4 directions and treat each space adjacent to the center of the node as a path if and only if it's adjacent to a traversible path


            if (WorldMapMatrix[(int)nodeMapPosition.X, (int)nodeMapPosition.Y - 2] == -1)
            {
                WorldMapMatrix[(int)nodeMapPosition.X, (int)nodeMapPosition.Y - 1] = -1;
                n.Visible = true;
            }

            if (WorldMapMatrix[(int)nodeMapPosition.X, (int)nodeMapPosition.Y + 2] == -1)
            {
                WorldMapMatrix[(int)nodeMapPosition.X, (int)nodeMapPosition.Y + 1] = -1;
                n.Visible = true;
            }

            if (WorldMapMatrix[(int)nodeMapPosition.X - 2, (int)nodeMapPosition.Y] == -1)
            {
                WorldMapMatrix[(int)nodeMapPosition.X - 1, (int)nodeMapPosition.Y] = -1;
                n.Visible = true;
            }

            if (WorldMapMatrix[(int)nodeMapPosition.X + 2, (int)nodeMapPosition.Y] == -1)
            {
                WorldMapMatrix[(int)nodeMapPosition.X + 1, (int)nodeMapPosition.Y] = -1;
                n.Visible = true;
            }

            if (reinitializing && n.Visible && !WasVisible)
            {
                n.StartAnimation();
            }

            n.SIDPrefix = SIDPrefix;
            n.InitializeNodeText();

        }

        private int CheckDirection(Vector2 pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    //if (pos.Y == 0)
                    //    return 0;
                    //else
                    return WorldMapMatrix[(int)pos.X, (int)pos.Y - 1];
                case Direction.Down:
                    //if (pos.Y == WorldMapMatrix.GetLength(1) - 1)
                    //    return 0;
                    //else
                    return WorldMapMatrix[(int)pos.X, (int)pos.Y + 1];
                case Direction.Left:
                    //if (pos.X == 0)
                    //    return 0;
                    //else
                    return WorldMapMatrix[(int)pos.X - 1, (int)pos.Y];
                case Direction.Right:
                    //if (pos.X == WorldMapMatrix.GetLength(0) - 1)
                    //    return 0;
                    //else
                    return WorldMapMatrix[(int)pos.X + 1, (int)pos.Y];
                default:
                    return 0;
            }
        }

        private void ShiftDestination(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    MapDestination.Y--;
                    Route.Add(new Vector2(0, -1));
                    break;
                case Direction.Down:
                    MapDestination.Y++;
                    Route.Add(new Vector2(0, 1));
                    break;
                case Direction.Left:
                    MapDestination.X--;
                    Route.Add(new Vector2(-1, 0));
                    break;
                case Direction.Right:
                    Route.Add(new Vector2(1, 0));
                    MapDestination.X++;
                    break;
            }
            MapDirection = dir;

        }

        private void OpenSelectedLevel()
        {
            String Destination = "";
            bool WarpNode = false;
            int ExitCount = 0;
            foreach (WorldMapNode n in _scene.Tracker.GetEntities<WorldMapNode>())
            {
                if (n.NodeID == WorldMapMatrix[(int)MapPosition.X, (int)MapPosition.Y])
                {
                    WarpNode = n.IsWarpNode;
                    if (n.IsWarpNode)
                        Destination = n.DestinationSID;
                    else
                    {
                        Destination = n.DestinationSID;
                        ExitCount = n.ExitCount;
                    }

                }


            }

            if (WarpNode)
            {
                WorldMapHelperModule.ModuleInstance.Teleport((_scene as Level), Destination);
            }
            else if (Destination != SIDPrefix && Destination != "")
            {
                WorldMapHelperModule.SaveData.AddLevelToJournal(Levelset, Destination, ExitCount);
                InGameOverworldHelper.OpenChapterPanel(_scene.Tracker.GetEntity<Player>(), SIDPrefix + Destination, Celeste.Mod.CollabUtils2.Triggers.ChapterPanelTrigger.ReturnToLobbyMode.SetReturnToHere,true, false);
                LevelDisplay.Visible = false;
            }


        }

        public void MoveToWarpDestination(string Destination)
        {
            foreach (WorldMapNode n in (_scene as Level).Tracker.GetEntities<WorldMapNode>())
            {
                if (n.WarpID == Destination)
                {
                    this.MapPosition = GameToMatrix(n.Position) + new Vector2(1, 1);
                    this.Position = MatrixToGame(this.MapPosition);
                }

            }

        }

        public void InitializeSprite(bool Badeline)
        {
            string characters = Badeline ? "WorldMapHelper_badeline" : "WorldMapHelper_player";
            WorldMapSprite = GFX.SpriteBank.Create(characters);
        }


        public void RunMapAnimations()
        {
            bool CurrentBatchDone;

            if (AnimationQueue.Count > 0)
            {
                if (AnimationClock >= 0)
                {
                    AnimationClock -= 1;
                    return;
                }
                else
                {


                    CurrentBatchDone = true;
                    foreach (WorldMapPath p in AnimationQueue)
                    {

                        if (p.Animating == AnimationState.NotYet && p.Priority == CurrentPriorityCount)
                        {
                            p.StartAnimation();
                            WorldMapHelperModule.SaveData.UnflagExit(p.Levelset, p.BoundExit);
                        }

                        if (p.Animating != AnimationState.Done && p.Priority == CurrentPriorityCount)
                            CurrentBatchDone = false;
                    }

                    if (CurrentBatchDone)
                    {
                        if (CurrentPriorityCount == AnimationQueue.Last().Priority)
                        {
                            MapCursorState = OverworldState.Ready;
                        }
                        else foreach (WorldMapPath p in AnimationQueue)
                            {
                                if (p.Priority > CurrentPriorityCount)
                                {
                                    CurrentPriorityCount = p.Priority;
                                    break;

                                }

                            }

                    }

                }



            }


        }


        public Vector2 AnimationTarget()
        {
            List<float> AvgX = new List<float>();
            List<float> AvgY = new List<float>();



            foreach (WorldMapPath p in AnimationQueue)
            {
                if (p.Animating == AnimationState.Animating)
                {
                    AvgX.Add(p.CameraTarget().X);
                    AvgY.Add(p.CameraTarget().Y);
                }
            }

            if (AvgX.Count > 0)
            {
                _LastAnimationTarget = CameraSafePosition(new Vector2(AvgX.Average(), AvgY.Average()));
            }

            return _LastAnimationTarget;

        }

        private void OpenJournal()

        {

            //
            MapCursorState = OverworldState.InMenu;
            LevelDisplay.Visible = false;
            InGameOverworldHelper.OpenJournal((_scene as Level).Tracker.GetEntity<Player>(), null);
            //hm.Unhook(typeof(Celeste.Mod.CollabUtils2.Triggers.JournalTrigger).GetMethod("onJournalEnter", BindingFlags.Static | BindingFlags.NonPublic));


        }


        public static void JournalHook(OuiJournal journal, Oui from)
        {

            for (int i = 0; i < journal.Pages.Count; i++)
            {
                if (journal.Pages[i].GetType() != typeof(OuiJournalCover))
                {
                    journal.Pages.RemoveAt(i);
                    i--;
                }
            }

            CreateJournal(journal, WorldMapHelperModule.ModuleInstance.ModName.Replace("/0-Lobbies/Overworld", ""));

        }

        public static void CreateJournal(OuiJournal journal, string Levelset)
        {
            JournalHandler jh = new JournalHandler(journal, Levelset);
            jh.CreatePages();
        }


        private void VisualizeMatrix()
        {
            string Row;

            for (int y = 0; y < WorldMapMatrix.GetLength(1); y++)
            {
                Row = "";
                for (int x = 0; x < WorldMapMatrix.GetLength(0); x++)
                {
                    if (x == (int)MapDestination.X && y == (int)MapDestination.Y)
                        Row += "*";
                    else
                        Row += WorldMapMatrix[x, y] == -1 ? "X" : " ";
                }
                System.Console.WriteLine(Row);
            }


        }


    }
}
