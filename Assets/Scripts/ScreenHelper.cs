using UnityEngine;

public static class ScreenHelper
{
    public static Vector2Int ScreenSize => new Vector2Int(Screen.width, Screen.height);
    public static float x = ScreenSize.x;
    public static float y = ScreenSize.y;
    public static float Aspect => (float)ScreenSize.y / ScreenSize.x;
    public static Vector2 GetNormalizedScreenPosition(RectTransform transform)
    {
        var output = transform.position;
        output.x /= x;
        output.y /= y;
        return output;
    }
}
