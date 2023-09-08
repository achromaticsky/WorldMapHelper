local path = {}

path.name = "WorldMapHelper/WorldMapPath"
path.placements = {
    name = "default",
    data = {
        BoundExit = "",
        UnlockEventDirection = "None",
        UnlockEventOrder = 0,
        Visible = 1,
        width = 8,
        height = 8
    }
}

path.fieldInformation = {
    UnlockEventDirection = {
        options = {
            "None",
            "Up",
            "Down",
            "Left",
            "Right"
        },
        editable = false
    }
}

path.fillColor = {1.0, 1.0, 0.5, 0.6}
path.borderColor = {1.0, 1.0, 0.5, 0.6}

return path