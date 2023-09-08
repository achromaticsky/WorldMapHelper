local node = {}
local utils = require("utils")

node.texture = "WorldMapHelper/node"
node.name = "WorldMapHelper/WorldMapNode"
node.placements = {
    name = "default",
    data = {
        Destination = "",
        StartingPoint = false,
        DisplayText = "",
        WarpID = "",
        ExitCount = 1,
        WarpNode = false,
        NodeColor = "Blue",
        CustomColor = "ffffff",
        CustomImage = ""
		
    }
	
}
node.offset = {0,0}

node.fieldInformation = {
    NodeColor = {
        options = {
            "Red",
            "Blue",
            "Green",
            "Custom"
        },
        editable = false
    },
    CustomColor = {
        fieldType = "color"
    }
}

function node.selection(room, entity)
    local rect = utils.rectangle(entity.x, entity.y, 24, 24)

    return rect
end


return node