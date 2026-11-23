using UnityEngine;

public class MissionBoard : MonoBehaviour
{
    [Header("Board Look")]
    public Color frameColor =
        new Color(0.24f, 0.12f, 0.05f, 1f);

    public Color paperColor =
        new Color(0.86f, 0.78f, 0.60f, 1f);

    public Color textColor =
        new Color(0.16f, 0.08f, 0.03f, 1f);

    bool built;

    void Awake()
    {
        BuildBoard();
    }

    public void Open()
    {
        if (MissionManager.Instance == null ||
            !MissionManager.Instance.IsReady)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowMessage(
                    "Requests unlock after the tutorial."
                );
            }

            return;
        }

        MissionNotebookUI.OpenNotebook();
    }

    void BuildBoard()
    {
        if (built)
            return;

        built = true;

        Material frameMaterial =
            CreateMaterial(frameColor);

        Material paperMaterial =
            CreateMaterial(paperColor);

        CreateCube(
            "Frame",
            new Vector3(0f, 1.55f, 0f),
            new Vector3(1.8f, 1.25f, 0.12f),
            frameMaterial
        );

        CreateCube(
            "Paper",
            new Vector3(0f, 1.55f, -0.07f),
            new Vector3(1.52f, 0.98f, 0.04f),
            paperMaterial
        );

        CreateCube(
            "Left Post",
            new Vector3(-0.65f, 0.75f, 0f),
            new Vector3(0.1f, 1.5f, 0.1f),
            frameMaterial
        );

        CreateCube(
            "Right Post",
            new Vector3(0.65f, 0.75f, 0f),
            new Vector3(0.1f, 1.5f, 0.1f),
            frameMaterial
        );

        CreateCube(
            "Top Trim",
            new Vector3(0f, 2.23f, 0f),
            new Vector3(1.9f, 0.12f, 0.16f),
            frameMaterial
        );

        CreateText(
            "Title",
            "REQUESTS",
            72,
            0.18f,
            new Vector3(0f, 1.73f, -0.11f)
        );

        CreateText(
            "Prompt",
            "E to read",
            48,
            0.10f,
            new Vector3(0f, 1.38f, -0.11f)
        );

        GameObject lightObject = new GameObject("Board Light");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition =
            new Vector3(0f, 2.1f, -1.15f);

        Light boardLight = lightObject.AddComponent<Light>();
        boardLight.type = LightType.Point;
        boardLight.color =
            new Color(1f, 0.78f, 0.45f, 1f);
        boardLight.intensity = 2.2f;
        boardLight.range = 4.5f;
        boardLight.shadows = LightShadows.None;

        BoxCollider collider =
            gameObject.GetComponent<BoxCollider>();

        if (collider == null)
            collider = gameObject.AddComponent<BoxCollider>();

        collider.center = new Vector3(0f, 1.25f, 0f);
        collider.size = new Vector3(1.9f, 2.4f, 0.7f);
    }

    GameObject CreateCube(
        string name,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        GameObject cube =
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.name = name;
        cube.transform.SetParent(transform, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;

        Renderer renderer =
            cube.GetComponent<Renderer>();

        if (renderer != null && material != null)
            renderer.sharedMaterial = material;

        Collider collider = cube.GetComponent<Collider>();

        if (collider != null)
            collider.enabled = false;

        return cube;
    }

    void CreateText(
        string name,
        string content,
        int fontSize,
        float characterSize,
        Vector3 position)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = position;

        TextMesh text = textObject.AddComponent<TextMesh>();
        Font font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.characterSize = characterSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = textColor;

        MeshRenderer renderer =
            textObject.GetComponent<MeshRenderer>();

        if (renderer != null)
            renderer.sharedMaterial = font.material;

        textObject.AddComponent<Billboard>();
    }

    static Material CreateMaterial(Color color)
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            return null;

        Material material = new Material(shader);

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.15f);

        return material;
    }
}
