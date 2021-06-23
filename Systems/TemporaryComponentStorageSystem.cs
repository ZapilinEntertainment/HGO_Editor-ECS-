using Unity.Entities;
using System.Collections.Generic;

public class TemporaryComponentStorageSystem : SystemBase
{
    private Dictionary<int, IComponentData> savedData;
    private int nextKey = 1;

    protected override void OnUpdate()
    {
        
    }

    public int SaveMyComponent(IComponentData data)
    {
        if (savedData == null) savedData = new Dictionary<int, IComponentData>();
        savedData.Add(nextKey, data);
        return nextKey++;
    }

    public bool TryGetMyComponent(int key, bool clearAfterRead, ref IComponentData data)
    {
        if (savedData == null) return false;
        else
        {
            if (savedData.ContainsKey(key))
            {
                data = savedData[key];
                if (clearAfterRead)
                {
                    savedData.Remove(key);
                    if (savedData.Count == 0) savedData = null;
                }
                return true;
            }
            else return false;
        }
    }
}
