using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
namespace WorldMapHelper.Entities
{
    [CustomEntity("WorldMapHelper/WorldMapNode")]
    [Tracked]
    class WorldMapNode : Solid
    {

        MTexture NodeImage;
        private int ID;
        private string SID;
        private bool Start;
        private int _ExitCount;
        private string _NodeText;
        private string _WarpID;
        private string _SIDPrefix;
        private bool _IsWarpNode;
        private string _NodeHexValue;
        private string _CustomImage;
        private Color color;
        private AnimationState _animating;
        private float _scale;
        private int _animationPhase;
        private int _clock = 40;
        //("Destination" => "", "StartingPoint" => false, "DisplayText" => "", "WarpID" => "", "ExitCount" => 1, "WarpNode" => false, "NodeColor" => "0000FF", "CustomColor" => "", "CustomImage" => "")
        public WorldMapNode(EntityData data, Vector2 offset, EntityID ID) : base(data.Position, data.Width, data.Height, true)
        {
            SID = data.Attr("Destination");
            Start = Convert.ToBoolean(data.Attr("StartingPoint"));
            NodeText = data.Attr("DisplayText");
            WarpID = data.Attr("WarpID");
            ExitCount = Convert.ToInt32(data.Attr("ExitCount"));
            IsWarpNode = Convert.ToBoolean(data.Attr("WarpNode"));
            CustomImage = data.Attr("CustomImage");
            if (data.Attr("NodeColor") == "")
                NodeHexValue = data.Attr("CustomColor");
            else
                NodeHexValue = data.Attr("NodeColor");
            _scale = 1;
            color = GetColorFromHexCode(NodeHexValue);

            if (CustomImage == "")
                NodeImage = GFX.Game["WorldMapHelper/node"];
            Position = data.Position + offset;
        }

        public int NodeID { get => ID; set => ID = value; }
        public string DestinationSID { get => SID; set => SID = value; }
        public bool StartingPoint { get => Start; set => Start = value; }
        public string NodeText { get => _NodeText; set => _NodeText = value; }
        public string WarpID { get => _WarpID; set => _WarpID = value; }
        public string SIDPrefix { get => _SIDPrefix; set => _SIDPrefix = value; }
        public bool IsWarpNode { get => _IsWarpNode; set => _IsWarpNode = value; }
        public string NodeHexValue { get => _NodeHexValue; set => _NodeHexValue = value; }
        public string CustomImage { get => _CustomImage; set => _CustomImage = value; }
        public int ExitCount { get => _ExitCount; set => _ExitCount = value; }

        public override void Render()
        {

            if (_animating == AnimationState.Animating)
            {
                if (_animationPhase == 0)
                    _clock--;

                if (_clock <= 0 && _animationPhase == 0)
                {
                    _animationPhase++;
                }

                if (_animationPhase == 1)
                {
                    _scale += 0.05f;
                }

                if (_animationPhase == 1 && _scale > 1.3)
                {
                    
                    SoundEmitter.Play("event:/game/04_cliffside/greenbooster_reappear", this);
                    _animationPhase++;
                }

                if (_animationPhase == 2 && _scale > 1)
                    _scale -= 0.05f;

                if (_animationPhase == 2 && _scale <= 1)
                {
                    _scale = 1;
                    _animating = AnimationState.Done;
                }
            }

            NodeImage.Draw(this.Position - new Vector2(-12, -12), new Vector2(12, 12), color, _scale);


            base.Render();

        }

        //Sound For Appearing
        //

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (NodeText == "")
            {
                if (DestinationSID != "")
                {
                    if (IsWarpNode)
                    {
                        NodeText = "To Overworld " + DestinationSID.Split('|')[0];
                    }
                    else
                    {
                        if (Dialog.Has((SIDPrefix + DestinationSID).Replace('/', '_')))
                        {

                            NodeText = Dialog.Get(SIDPrefix + DestinationSID.Replace('/', '_'));

                        }
                        else
                        {
                            NodeText = DestinationSID;
                        }
                    }
                }
            }

            if (CustomImage != "")
                NodeImage = GFX.Game["WorldMapHelper/" + CustomImage];

            else
            {
                NodeImage = GFX.Game["WorldMapHelper/node"];
            }


        }

        public void StartAnimation()
        {

            _scale = 0;
            _animating = AnimationState.Animating;
            _animationPhase = 0;
        }


        private Color GetColorFromHexCode(string HexCode)
        {
            int R, G, B;

            try
            {
                R = int.Parse(HexCode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                G = int.Parse(HexCode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                B = int.Parse(HexCode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                return Color.White;
            }

            return new Color(R, G, B);
        }




        public void InitializeNodeText()
        {
            //Set default text if the node text is left blank, if the node is a level node, it's the name of the level. 
            if (NodeText == "")
            {
                if (DestinationSID != "")
                {
                    if (IsWarpNode)
                    {
                        NodeText = "To Overworld " + DestinationSID.Split('|')[0];
                    }
                    else
                    {
                        if (Dialog.Has((SIDPrefix + DestinationSID).Replace('/', '_')))
                        {
                            NodeText = Dialog.Get(SIDPrefix + DestinationSID.Replace('/', '_'));
                        }
                        else
                        {
                            NodeText = DestinationSID;
                        }
                    }
                }
            }

        }



        public override void Update()
        {

            base.Update();

        }

    }
}
