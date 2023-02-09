module WorldMapHelper

using ..Ahorn, Maple

@mapdef Entity "WorldMapHelper/WorldMapController" WorldMapBrain(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
   "World Map Controller" => Ahorn.EntityPlacement(
      WorldMapController
   )
)

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WorldMapController, room::Maple.Room) = Ahorn.drawRectangle(ctx,0,0,8,8)




end