using UnityEngine;
public static class MyLog
{
    public static void Print(float a, float b)
    {
        Debug.Log(a.ToString() + ", " + b.ToString());
    }
    public static void Print(int a, int b)
    {
        Debug.Log(a.ToString() + ", " + b.ToString());
    }
    public static void PrintInt(float a, float b)
    {
        Debug.Log(((int)a).ToString() + ", " + ((int)b).ToString());
    }
}
