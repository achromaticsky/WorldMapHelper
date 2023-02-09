module WorldMapHelper

using ..Ahorn, Maple

@mapdef Entity "WorldMapHelper/WorldMapPath" WorldMapPath(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
   "World Map Path" => Ahorn.EntityPlacement(
      WorldMapPath,
	  "rectangle",
	  Dict{String, Any}("BoundExit" => ""),
	  Ahorn.tileEntityFinalizer	  
   )
)


Ahorn.minimumSize(entity::WorldMapPath) = 8, 8
Ahorn.resizable(entity::WorldMapPath) = true, true

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::WorldMapPath, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)



end