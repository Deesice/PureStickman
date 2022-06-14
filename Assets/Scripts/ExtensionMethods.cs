using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ExtensionMethods
{
    public static Vector3 IntersectionPoint(Vector3 r, Vector3 r2, Vector3 normal, Vector3 anyPointOnPlane)
    {
        var s = r - r2;
        var d = anyPointOnPlane.x * normal.x + anyPointOnPlane.y * normal.y + anyPointOnPlane.z * normal.z;

        return r - (Vector3.Dot(r, normal) - d) / Vector3.Dot(s, normal) * s;
    }
    public static T AddComponentOrGetIfExists<T>(this GameObject g) where T: Component
    {
        var c = g.GetComponent<T>();
        if (c)
            return c;
        else
            return g.AddComponent<T>();
    }
    public static T Random<T>(this T[] array, System.Predicate<T> p)
    {
        if (array.Length > 0)
        {
            bool flag = true;
            T selected = default(T);
            while (flag)
            {
                selected = array[UnityEngine.Random.Range(0, array.Length)];
                if (p(selected))
                    flag = false;
            }
            return selected;
        }
        else
            return default(T);
    }
    public static T Gradient<T>(this T[] array, float value)
    {
        return array[Mathf.RoundToInt(Mathf.Lerp(0,
                array.Length - 1,
                value))];
    }
    public static T Random<T>(this T[] array)
    {
        return array.Random(p => true);
    }
    public static T Random<T>(this List<T> array) where T : UnityEngine.Object
    {
        if (array.Count > 0)
            return array[UnityEngine.Random.Range(0, array.Count)];
        else
            return null;
    }
    public static void PrintError(this GameObject gameObject, string message)
    {
        Debug.LogError(message);
    }
    public static void PrintName(this GameObject gameObject)
    {
        Debug.Log(gameObject.name);
    }
    public static Vector3 LocalToWorld(this Transform transform, Vector3 local)
    {
        return transform.forward * local.z + transform.right * local.x + transform.up * local.y;
    }
    public static float GetRelative4K(this CanvasScaler canvas)
    {
        var targetValue = Mathf.Lerp(canvas.referenceResolution.x, canvas.referenceResolution.y, canvas.matchWidthOrHeight);
        var currentValue = Mathf.Lerp(Screen.width, Screen.height, canvas.matchWidthOrHeight);
        return currentValue / targetValue;
    }
    public static string GetContentByURL(string url)
    {
        var client = new WebClient();
        var stream = client.OpenRead(url);
        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    public static bool Contains(this RectTransform transform, Vector3 pos, float relative4K = 1)
    {
        var maxScreenCorner = (Vector2)transform.position + transform.rect.max * relative4K;
        var minScreenCorner = (Vector2)transform.position + transform.rect.min * relative4K;

        return pos.x > minScreenCorner.x
            && pos.x < maxScreenCorner.x
            && pos.y > minScreenCorner.y
            && pos.y < maxScreenCorner.y;
    }
    public static bool Contains(this RectTransform transform, Vector3 pos, CanvasScaler canvasScaler)
    {
        return transform.Contains(pos, canvasScaler.GetRelative4K());
    }
    public static void SetDefaults(this AudioSource source)
    {
        source.spatialBlend = 1;
        source.dopplerLevel = 0;
        source.spread = 0;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 6;
        source.maxDistance = 12;
    }
    public static string GetHierarchyPath<T>(this T behaviour) where T : Component
    {
        string output = "";
        var t = behaviour.transform;
        while (t != null)
        {
            output = t.gameObject.name + "/" + output;
            t = t.parent;
        }
        output = "PERMAPVALUE/" + SceneManager.GetActiveScene().name + "/" + output;
        output += typeof(T);
        int count = 0;
        foreach (var i in behaviour.gameObject.GetComponents<T>())
            if (i == behaviour)
                return output + "_" + count;
            else
                count++;
        throw new NotImplementedException();
    }
    public static bool Contains<T>(this List<T> list, Predicate<T> p)
    {
        foreach (var i in list)
            if (p(i))
                return true;
        return false;
    }
    public static Vector2 CustomWorldToViewportPoint(this Camera cam, Vector3 worldPos)
    {
        var fromCamera = (worldPos - cam.transform.position);
        var dot = Vector3.Dot(cam.transform.forward, fromCamera.normalized);
        var angle = Mathf.Deg2Rad * cam.fieldOfView * cam.aspect / 2;

        if (dot < Mathf.Cos(angle))
        {
            var offsetDirecton = Vector3.Cross(fromCamera, cam.transform.up);
            if (Vector3.Dot(cam.transform.right, fromCamera) > 0)
                worldPos += Mathf.Tan(Mathf.Acos(dot) - angle) * offsetDirecton * cam.aspect;
            else
                worldPos -= Mathf.Tan(Mathf.Acos(dot) - angle) * offsetDirecton * cam.aspect;
        }

        return cam.WorldToViewportPoint(worldPos);
    }
}
