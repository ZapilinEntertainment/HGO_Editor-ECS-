using Unity.Physics;
public enum MyCollisionFilters : byte { Default, EditorSelectedObject}
public enum MyCollisionLayer : byte { Parts, SystemObjects}

//https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/UnityPhysicsSamples/Assets/Demos/4.%20Joints/Scripts/RagdollDemo.cs

public static class MyCollisionLayerExtension {

    public static CollisionFilter allFilter = new CollisionFilter()
    {
        BelongsTo = (1 << 0),
        CollidesWith = (1 << 0) + (1 << 1),
        GroupIndex = 0
    };
    public static CollisionFilter selectedFilter = new CollisionFilter()
    {
        BelongsTo = (uint)(1 << 1),
        CollidesWith = (1 << 1),
        GroupIndex = 0
    };
    public static CollisionFilter nonSelectedFilter = new CollisionFilter()
    {
        BelongsTo = (uint)(1 << 0),
        CollidesWith = (1 << 0),
        GroupIndex = 0
    };
    


    public static CollisionFilter GetCollisionFilter(this MyCollisionFilters cl)
    {
        uint belongs = 0, disables = 0;
        if (cl != MyCollisionFilters.Default)
        {
            belongs = 1 << 1;
            disables = 1 << 0;
        }
        return new CollisionFilter()
        {
            BelongsTo = belongs,
            CollidesWith = ~disables,
            GroupIndex = 0
        };
    }

}
