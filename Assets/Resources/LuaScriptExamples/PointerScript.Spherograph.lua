Settings = {
    description="",
    space="canvas"
}

Parameters = {
    uScale={label="U Scaling", type="float", min=0.001, max=1, default=1},
    vScale={label="V Scaling", type="float", min=0.001, max=1, default=0.1},
    radius={label="Radius", type="float", min=0, max=20, default=5},
}

function Main()
    if Brush.triggerPressedThisFrame then
        initialPos = Brush.position
    elseif Brush.triggerIsPressed then
        u = Brush.distanceDrawn * uScale
        v = Brush.distanceDrawn * vScale
        p = Sphere(u,v)
        return Transform:New(
            initialPos:Add(p),
            Rotation.lookRotation(p, Vector3.right)
        )
    end
end

function Sphere(u, v)
    return Vector3:New(
        Math:Cos(u) * Math:Cos(v),
        Math:Cos(u) * Math:Sin(v),
        Math:Sin(u)
    )
end
