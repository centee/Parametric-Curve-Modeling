using System.Collections.Generic;
using UnityEngine;

public enum CurveType
{
    Bezier2,    //二阶贝塞尔
    Bezier3,    //三阶贝塞尔
    CatmullRom, //Catmull-Rom插值样条
    BSpline     //三次B样条
}

[RequireComponent(typeof(LineRenderer))]
public class CurveManager : MonoBehaviour
{
    [Header("曲线设置")]
    public CurveType curveType;
    public int sampleCount = 100;
    public GameObject controlPointPrefab;
    public List<Transform> controlPoints = new List<Transform>();

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        DrawCurve();
    }

    void DrawCurve()
    {
        if (controlPoints.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        List<Vector3> curvePts = new List<Vector3>();

        switch (curveType)
        {
            case CurveType.Bezier2:
                if (controlPoints.Count >= 3)
                {
                    for (int i = 0; i <= sampleCount; i++)
                    {
                        float t = i / (float)sampleCount;
                        curvePts.Add(Bezier2(controlPoints[0].position, controlPoints[1].position, controlPoints[2].position, t));
                    }
                }
                break;
            case CurveType.Bezier3:
                if (controlPoints.Count >= 4)
                {
                    for (int i = 0; i <= sampleCount; i++)
                    {
                        float t = i / (float)sampleCount;
                        curvePts.Add(Bezier3(controlPoints[0].position, controlPoints[1].position, controlPoints[2].position, controlPoints[3].position, t));
                    }
                }
                break;
            case CurveType.CatmullRom:
                if (controlPoints.Count >= 4)
                {
                    //分段CatmullRom
                    for (int seg = 1; seg < controlPoints.Count - 2; seg++)
                    {
                        Vector3 p0 = controlPoints[seg - 1].position;
                        Vector3 p1 = controlPoints[seg].position;
                        Vector3 p2 = controlPoints[seg + 1].position;
                        Vector3 p3 = controlPoints[seg + 2].position;
                        for (int j = 0; j <= sampleCount; j++)
                        {
                            float t = j / (float)sampleCount;
                            curvePts.Add(CatmullRom(p0, p1, p2, p3, t));
                        }
                    }
                }
                break;
            case CurveType.BSpline:
                if (controlPoints.Count >= 4)
                {
                    for (int i = 0; i <= sampleCount; i++)
                    {
                        float t = i / (float)sampleCount * (controlPoints.Count - 3);
                        curvePts.Add(BSpline3(controlPoints, t));
                    }
                }
                break;
        }

        lineRenderer.positionCount = curvePts.Count;
        for (int i = 0; i < curvePts.Count; i++)
        {
            lineRenderer.SetPosition(i, curvePts[i]);
        }
    }

    //二阶贝塞尔
    Vector3 Bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float mt = 1 - t;
        return mt * mt * p0 + 2 * mt * t * p1 + t * t * p2;
    }

    //三阶贝塞尔
    Vector3 Bezier3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float mt = 1 - t;
        return mt * mt * mt * p0
            + 3 * mt * mt * t * p1
            + 3 * mt * t * t * p2
            + t * t * t * p3;
    }

    //CatmullRom样条
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3
            + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2
            + (-p0 + p2) * t
            + 2 * p1
        );
    }

    //三次B样条
    Vector3 BSpline3(List<Transform> pts, float t)
    {
        int i = Mathf.FloorToInt(t);
        t -= i;
        i = Mathf.Clamp(i, 0, pts.Count - 4);
        Vector3 p0 = pts[i].position;
        Vector3 p1 = pts[i + 1].position;
        Vector3 p2 = pts[i + 2].position;
        Vector3 p3 = pts[i + 3].position;

        float t2 = t * t;
        float t3 = t2 * t;
        return 1f / 6f * (
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3
            + (3 * p0 - 6 * p1 + 3 * p2) * t2
            + (-3 * p0 + 3 * p2) * t
            + (p0 + 4 * p1 + p2)
        );
    }

    [ContextMenu("Add Control Point")]
    public void AddControlPoint()
    {
        GameObject pt = Instantiate(controlPointPrefab, transform);
        pt.transform.position = new Vector3(controlPoints.Count * 1.5f, 0, 0);
        pt.name = "Point_" + controlPoints.Count;
        controlPoints.Add(pt.transform);
    }

    [ContextMenu("Remove Last Point")]
    public void RemoveLastPoint()
    {
        if (controlPoints.Count == 0) return;
        Transform last = controlPoints[controlPoints.Count - 1];
        controlPoints.Remove(last);
        DestroyImmediate(last.gameObject);
    }

    [ContextMenu("Clear All Points")]
    public void ClearPoints()
    {
        foreach (var p in controlPoints) DestroyImmediate(p.gameObject);
        controlPoints.Clear();
    }
}
