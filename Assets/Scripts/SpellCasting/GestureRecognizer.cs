using UnityEngine;
using System.Collections.Generic;

public static class GestureRecognizer
{
    public static (bool isMatch, float score, Vector2 center, float radius) IsCircle2D(List<Vector2> points, float minRadius = 0.3f, float maxRadius = 0.8f)
    {
        if (points.Count < 8) return (false, 0f, Vector2.zero, 0f);

        Vector2 center = Vector2.zero;
        foreach (var p in points) center += p;
        center /= points.Count;

        float avgRadius = 0f;
        float minR = float.MaxValue, maxR = 0f;
        foreach (var p in points)
        {
            float r = Vector2.Distance(p, center);
            avgRadius += r;
            minR = Mathf.Min(minR, r);
            maxR = Mathf.Max(maxR, r);
        }
        avgRadius /= points.Count;

        if (avgRadius < minRadius || avgRadius > maxRadius) return (false, 0f, center, avgRadius);

        float deviation = (maxR - minR) / avgRadius;
        float score = 1f - deviation;

        return (score > 0.7f, score, center, avgRadius);
    }

    public static (bool isMatch, float score, Vector2 start, Vector2 end) IsLine2D(List<Vector2> points)
    {
        if (points.Count < 5) return (false, 0f, Vector2.zero, Vector2.zero);

        Vector2 start = points[0];
        Vector2 end = points[points.Count - 1];

        Vector2 direction = end - start;
        float lengthSq = direction.sqrMagnitude;
        if (lengthSq < 0.001f) return (false, 0f, start, end);

        float maxDeviation = 0f;
        for (int i = 1; i < points.Count - 1; i++)
        {
            float dot = Vector2.Dot(points[i] - start, direction);
            Vector2 proj = start + (dot / lengthSq) * direction;
            maxDeviation = Mathf.Max(maxDeviation, Vector2.Distance(points[i], proj));
        }

        float length = Mathf.Sqrt(lengthSq);
        float score = 1f - (maxDeviation / length);
        return (score > 0.7f, score, start, end);
    }

    public static (bool isMatch, float score, Vector3 center, float radius) IsCircle(List<Vector3> points, float minRadius = 0.5f, float maxRadius = 2f)
    {
        List<Vector2> points2D = new List<Vector2>();
        foreach (var p in points) points2D.Add(new Vector2(p.x, p.y));
        var result2D = IsCircle2D(points2D, minRadius, maxRadius);
        return (result2D.isMatch, result2D.score, new Vector3(result2D.center.x, result2D.center.y, 0), result2D.radius);
    }

    public static (bool isMatch, float score, Vector3 start, Vector3 end) IsLine(List<Vector3> points)
    {
        List<Vector2> points2D = new List<Vector2>();
        foreach (var p in points) points2D.Add(new Vector2(p.x, p.y));
        var result2D = IsLine2D(points2D);
        return (result2D.isMatch, result2D.score, new Vector3(result2D.start.x, result2D.start.y, 0), new Vector3(result2D.end.x, result2D.end.y, 0));
    }
}