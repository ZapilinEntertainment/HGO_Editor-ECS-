using Unity.Physics;

//https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/UnityPhysicsSamples/Assets/Demos/4.%20Joints/Scripts/RagdollDemo.cs

public static class MyCollisionLayerExtension {

    public static CollisionFilter allDetailsFilter = new CollisionFilter()
    {
        BelongsTo = (1 << 0) + (1<< 1),
        CollidesWith = (1 << 0) + (1 << 1),
        GroupIndex = 0
    };
    public static CollisionFilter selectedDetailsFilter = new CollisionFilter()
    {
        BelongsTo = (uint)(1 << 1),
        CollidesWith = (1 << 1),
        GroupIndex = 0
    };
    public static CollisionFilter nonSelectedDetailsFilter = new CollisionFilter()
    {
        BelongsTo = (uint)(1 << 0),
        CollidesWith = (1 << 0),
        GroupIndex = 0
    };
    public static CollisionFilter controlElementsLayer = new CollisionFilter()
    {
        BelongsTo = (uint)(1 << 2),
        CollidesWith = (1 << 2),
        GroupIndex = 0
    };

}
