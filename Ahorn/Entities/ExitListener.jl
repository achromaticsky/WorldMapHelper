module WorldMapHelper

using ..Ahorn, Maple

@mapdef Entity "WorldMapHelper/ExitListener" ExitListener(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
   "Exit Listener" => Ahorn.EntityPlacement(
      ExitListener
   )
)

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ExitListener, room::Maple.Room) = Ahorn.drawRectangle(ctx,0,0,8,8)




end