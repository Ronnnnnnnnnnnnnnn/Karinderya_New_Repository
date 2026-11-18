using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Draws a breadcrumb trail of chevrons on the ground for the tutorial.
/// It follows the same target transform as the tutorial's world marker.
/// NavMesh paths are used when available; otherwise it falls back to a
/// straight ground line.
/// </summary>
public class TutorialPathGuide : MonoBehaviour
{
    const int MaxArrows = 12;

    [Header("Path")]
    public float spacing = 1.15f;
    public float startOffset = 0.9f;
    public float endOffset = 0.55f;
    public float fallbackLength = 4.5f;
    public float groundOffset = 0.035f;
    public float refreshInterval = 0.2f;

    [Header("Arrow Visual")]
    public Color arrowColor = new Color(0.96f, 0.76f, 0.28f, 1f);
    public float pulseSpeed = 4f;

    Transform player;
    Transform target;

    bool visible;
    float refreshTimer;

    GameObject[] arrows;
    Mesh arrowMesh;
    Material arrowMaterial;
    bool initialized;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;

        if (initialized)
            return;

        initialized = true;

        BuildMesh();
        BuildMaterial();
        BuildArrows();

        HideAll();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        refreshTimer = 0f;

        if (visible)
            UpdatePath();
        else
            HideAll();
    }

    public void SetVisible(bool value)
    {
        visible = value;

        if (!visible)
        {
            HideAll();
            return;
        }

        refreshTimer = 0f;
        UpdatePath();
    }

    void Update()
    {
        if (!visible || target == null || player == null)
        {
            HideAll();
            return;
        }

        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            UpdatePath();
        }

        AnimateArrows();
    }

    // =========================================================
    // PATH
    // =========================================================

    void UpdatePath()
    {
        if (arrows == null || arrows.Length == 0)
            return;

        HideAll();

        if (!visible || target == null || player == null)
            return;

        Vector3 start = player.position;
        Vector3 end = target.position;

        Vector3 flat = end - start;
        flat.y = 0f;

        float straightDistance = flat.magnitude;

        if (straightDistance < 1.3f)
            return;

        Vector3 straightDirection = flat.normalized;
        start += straightDirection * startOffset;
        end -= straightDirection * endOffset;

        if (TryDrawNavMeshPath(start, end))
            return;

        DrawStraightPath(start, end);
    }

    bool TryDrawNavMeshPath(Vector3 start, Vector3 end)
    {
        if (!NavMesh.SamplePosition(
                start,
                out NavMeshHit startHit,
                2f,
                NavMesh.AllAreas))
        {
            return false;
        }

        // Don't start the trail on a far-away NavMesh island.
        if (Vector3.Distance(startHit.position, start) > 1.5f)
            return false;

        if (!NavMesh.SamplePosition(
                end,
                out NavMeshHit endHit,
                2f,
                NavMesh.AllAreas))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();

        if (!NavMesh.CalculatePath(
                startHit.position,
                endHit.position,
                NavMesh.AllAreas,
                path))
        {
            return false;
        }

        if (path.status != NavMeshPathStatus.PathComplete ||
            path.corners == null ||
            path.corners.Length < 2)
        {
            return false;
        }

        float totalLength = 0f;

        for (int i = 1; i < path.corners.Length; i++)
            totalLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);

        if (totalLength < 1.3f)
            return false;

        int count = Mathf.Clamp(
            Mathf.CeilToInt(totalLength / spacing),
            1,
            MaxArrows
        );

        for (int i = 0; i < count; i++)
        {
            float distance = totalLength * (i + 1f) / (count + 1f);

            if (!TryPointOnPath(
                    path.corners,
                    distance,
                    out Vector3 point,
                    out Vector3 direction))
            {
                continue;
            }

            PlaceArrow(i, point, direction);
        }

        return true;
    }

    void DrawStraightPath(Vector3 start, Vector3 end)
    {
        Vector3 flat = end - start;
        flat.y = 0f;

        float length = flat.magnitude;

        if (length < 0.4f)
            return;

        Vector3 direction = flat.normalized;

        // If there is no NavMesh path, only draw a short directional
        // trail. A full straight line can cut through walls.
        if (length > fallbackLength)
            length = fallbackLength;

        int count = Mathf.Clamp(
            Mathf.CeilToInt(length / spacing),
            1,
            MaxArrows
        );

        for (int i = 0; i < count; i++)
        {
            float t = (i + 1f) / (count + 1f);
            Vector3 point =
                start + direction * (length * t);

            PlaceArrow(i, point, direction);
        }
    }

    static bool TryPointOnPath(
        Vector3[] corners,
        float distance,
        out Vector3 point,
        out Vector3 direction)
    {
        point = Vector3.zero;
        direction = Vector3.forward;

        float walked = 0f;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 segment = corners[i] - corners[i - 1];
            float segmentLength = segment.magnitude;

            if (segmentLength <= 0.0001f)
                continue;

            if (walked + segmentLength >= distance)
            {
                float t = (distance - walked) / segmentLength;
                point = Vector3.Lerp(corners[i - 1], corners[i], t);
                direction = segment.normalized;
                return true;
            }

            walked += segmentLength;
        }

        return false;
    }

    void PlaceArrow(int index, Vector3 point, Vector3 direction)
    {
        if (arrows == null || index < 0 || index >= arrows.Length)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;

        direction.Normalize();

        float rayStartY =
            Mathf.Max(player.position.y, point.y) + 2f;

        Vector3 rayOrigin = new Vector3(
            point.x,
            rayStartY,
            point.z
        );

        if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                8f,
                ~0,
                QueryTriggerInteraction.Ignore))
        {
            float floorHeight =
                player.position.y + groundOffset;

            // Ignore tables/roofs/characters above the floor; the trail
            // should stay on the walking surface.
            if (hit.point.y > player.position.y + 0.75f)
                point.y = floorHeight;
            else
                point.y = hit.point.y + groundOffset;
        }
        else
        {
            point.y = player.position.y + groundOffset;
        }

        GameObject arrow = arrows[index];

        arrow.transform.position = point;
        arrow.transform.rotation =
            Quaternion.LookRotation(direction, Vector3.up);
        arrow.transform.localScale = Vector3.one;
        arrow.SetActive(true);
    }

    void AnimateArrows()
    {
        if (arrows == null)
            return;

        for (int i = 0; i < arrows.Length; i++)
        {
            GameObject arrow = arrows[i];

            if (arrow == null || !arrow.activeSelf)
                continue;

            float pulse =
                0.88f +
                0.12f *
                Mathf.Sin(
                    Time.time * pulseSpeed -
                    i * 0.55f
                );

            arrow.transform.localScale =
                new Vector3(pulse, 1f, pulse);
        }
    }

    void HideAll()
    {
        if (arrows == null)
            return;

        foreach (GameObject arrow in arrows)
        {
            if (arrow != null)
                arrow.SetActive(false);
        }
    }

    // =========================================================
    // BUILD
    // =========================================================

    void BuildArrows()
    {
        arrows = new GameObject[MaxArrows];

        for (int i = 0; i < MaxArrows; i++)
        {
            GameObject arrow = new GameObject(
                "TutorialPathArrow",
                typeof(MeshFilter),
                typeof(MeshRenderer)
            );

            arrow.transform.SetParent(transform, false);

            MeshFilter filter = arrow.GetComponent<MeshFilter>();
            filter.sharedMesh = arrowMesh;

            MeshRenderer renderer = arrow.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = arrowMaterial;
            renderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            arrow.SetActive(false);
            arrows[i] = arrow;
        }
    }

    void BuildMesh()
    {
        arrowMesh = new Mesh();
        arrowMesh.name = "TutorialPathChevron";

        // Flat chevron pointing toward +Z.
        Vector3[] vertices =
        {
            new Vector3(0f, 0f, 0.30f),
            new Vector3(-0.24f, 0f, -0.10f),
            new Vector3(-0.12f, 0f, -0.10f),
            new Vector3(0f, 0f, 0.02f),
            new Vector3(0.12f, 0f, -0.10f),
            new Vector3(0.24f, 0f, -0.10f)
        };

        int[] triangles =
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5
        };

        arrowMesh.vertices = vertices;
        arrowMesh.triangles = triangles;
        arrowMesh.RecalculateNormals();
        arrowMesh.RecalculateBounds();
    }

    void BuildMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader == null)
            return;

        arrowMaterial = new Material(shader);
        arrowMaterial.name = "TutorialPathArrowMaterial";

        arrowMaterial.color = arrowColor;
        arrowMaterial.SetColor("_BaseColor", arrowColor);
        arrowMaterial.SetColor("_Color", arrowColor);
    }
}
