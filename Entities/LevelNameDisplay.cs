using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace WorldMapHelper.Entities
{
    [CustomEntity("WorldMapHelper/LevelNameDisplay")]
    [Tracked]
    class LevelNameDisplay : Entity
    {

        private string Text;


        public LevelNameDisplay(Vector2 position) : base(position)
        {
            this.AddTag(Tags.HUD);
            //Text = "This hasn't been properly initialized.";

        }

        public string Text1 { get => Text; set => Text = value; }

        public override void Render()
        {
            Celeste.Fonts.Get("Renogare").DrawOutline(4, Text, new Vector2(1890f, 20f), new Vector2(1, 0), new Vector2(20f, 20f), Color.White, 1f, Color.Black);
            //ActiveFont.DrawOutline(Text, new Vector2(20, 20), new Vector2(0, 0), new Vector2(20f, 20f), Color.White, 1f, Color.Black);
            base.Render();
        }


    }

}
