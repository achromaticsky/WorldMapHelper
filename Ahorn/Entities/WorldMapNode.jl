module WorldMapHelper

using ..Ahorn, Maple

@mapdef Entity "WorldMapHelper/WorldMapNode" WorldMapNode(x::Integer, y::Integer)

sprite = "node_blue"

const placements = Ahorn.PlacementDict(
   "World Map Node" => Ahorn.EntityPlacement(
      WorldMapNode
   )
)

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::YourEntity, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0,0)




end