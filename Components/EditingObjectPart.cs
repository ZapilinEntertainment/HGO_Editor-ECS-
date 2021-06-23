using Unity.Entities;

public struct EditingObjectPart : IComponentData
{
    public int index { get; private set; }

    public void AssignIndex(int i)
    {
        index = i;
    }
}
