using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.WorldMapHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;


namespace WorldMapHelper.Entities
{
    [CustomEntity("WorldMapHelper/WorldMapPath")]
    [Tracked]
    class WorldMapPath : Solid, IComparable<WorldMapPath>
    {


        Scene _scene;
        Color _currentColor;

        bool _Visible = true;
        string _BoundExit;
        string _Levelset;
        private AnimationState _Animating;
        int _Priority;
        float _DistanceDrawn;
        float DrawSpeed = 1f;
        Direction pathDirection;
        public string Levelset { get => _Levelset; set => _Levelset = value; }
        public AnimationState Animating { get => _Animating; }
        public int Priority { get => _Priority; set => _Priority = value; }
        public string BoundExit { get => _BoundExit; set => _BoundExit = value; }

       // public int AlwaysInvisible 

        public WorldMapPath(EntityData data, Vector2 offset, EntityID ID) : base(data.Position, data.Width, data.Height, true)
        {
            Visible = data.Attr("VisibleWhenUnlocked") == "" ? true : bool.Parse(data.Attr("VisibleWhenUnlocked"));
         

            System.Console.WriteLine(data.Attr("Visible"));
            BoundExit = data.Attr("BoundExit");
            Position = data.Position + offset;
            _Priority = int.Parse(data.Attr("UnlockEventOrder"));

            switch (data.Attr("UnlockEventDirection"))
            {
                case "Up":
                    pathDirection = Direction.Up;
                    break;
                case "Down":
                    pathDirection = Direction.Down;
                    break;
                case "Left":
                    pathDirection = Direction.Left;
                    break;
                case "Right":
                    pathDirection = Direction.Right;
                    break;
                case "None":
                    pathDirection = Direction.None;
                    break;
            }

        }

        public int CompareTo(WorldMapPath comparison)
        {
            if (this.Priority > comparison.Priority)
                return 1;
            else if (this.Priority < comparison.Priority)
                return -1;
            else
                return 0;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _scene = scene;

            Levelset = (scene as Level).Session.Area.LevelSet.Split('/')[0];
            //Visible = false;
            //StartAnimation(Direction.None);

            //scene.Add(this);
        }



        public override void Render()
        {
            if (_Visible)
            {

            
            _currentColor = (_scene as Level).Tracker.GetEntity<WorldMapController>().PathCurrentColor;


            if (_Animating == AnimationState.Animating)
            {
                switch (pathDirection)
                {
                    case Direction.Up:
                        Draw.Rect(this.Position + new Vector2(0, this.Height - _DistanceDrawn), this.Width, _DistanceDrawn, _currentColor);
                        _DistanceDrawn += DrawSpeed;
                        if (_DistanceDrawn >= this.Height)
                            _Animating = AnimationState.Done;
                        break;
                    case Direction.Down:
                        Draw.Rect(this.Position, this.Width, _DistanceDrawn, _currentColor);
                        _DistanceDrawn += DrawSpeed;
                        if (_DistanceDrawn >= this.Height)
                            _Animating = AnimationState.Done;
                        break;
                    case Direction.Left:
                        Draw.Rect(this.Position + new Vector2(this.Width - _DistanceDrawn, 0), _DistanceDrawn, this.Height, _currentColor);
                        _DistanceDrawn += DrawSpeed;
                        if (_DistanceDrawn >= this.Width)
                            _Animating = AnimationState.Done;
                        break;
                    case Direction.Right:
                        Draw.Rect(this.Position, _DistanceDrawn, this.Height, _currentColor);
                        _DistanceDrawn += DrawSpeed;
                        if (_DistanceDrawn >= this.Width)
                            _Animating = AnimationState.Done;
                        break;
                    case Direction.None:
                        Draw.Rect(this.Position, this.Width, this.Height, _currentColor * (_DistanceDrawn / 100f) * 0.7f);
                        _DistanceDrawn += DrawSpeed;
                        if (_DistanceDrawn >= 100)
                            _Animating = AnimationState.Done;
                        break;
                }
            }
            else
            {
                Draw.Rect(this.Position, this.Width, this.Height, _currentColor);
            }

            this.Depth = -10000;
            base.Render();
            }
        }

        public void StartAnimation()
        {
            _Animating = AnimationState.Animating;
            _DistanceDrawn = 0;
            Visible = true;
            //SoundEmitter.Play("")

        }


        public Vector2 CameraTarget()
        {

            switch (pathDirection)
            {
                case Direction.Up:
                    return this.Position + new Vector2(0, this.Height - _DistanceDrawn);
                case Direction.Down:
                    return this.Position + new Vector2(0, _DistanceDrawn);
                case Direction.Left:
                    return this.Position + new Vector2(this.Width - _DistanceDrawn, 0);
                case Direction.Right:
                    return this.Position + new Vector2(_DistanceDrawn, 0);
                default:
                    return this.Position + new Vector2(this.Width / 2, this.Height / 2);
            }



        }

        public PathState CheckOpen()
        {
            if (WorldMapHelperModule.SaveData.CheckExit(_Levelset, BoundExit) || BoundExit == "")
            {

                if (WorldMapHelperModule.SaveData.IsExitNew(_Levelset, BoundExit))
                {
                    _Animating = AnimationState.NotYet;
                    Visible = false;
                    return PathState.NewlyOpen;
                }
                else
                {
                    return PathState.Open;
                }
            }
            else
            {
                Visible = false;
                return PathState.Closed;

            }
        }



        public override void Update()
        {

            base.Update();

        }

    }
}
