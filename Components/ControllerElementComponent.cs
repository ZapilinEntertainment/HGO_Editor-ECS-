using Unity.Entities;

public enum ControllerElementType : byte { RotatingWheel, ScaleArrow}
public struct ControllerElementComponent : IComponentData
{
    public ControllerElementType type;
    public int ID { get; private set; }
    private static int nextID = 10;

    public void SYSTEM_AssignID()
    {
        ID = nextID++;
    }
}
