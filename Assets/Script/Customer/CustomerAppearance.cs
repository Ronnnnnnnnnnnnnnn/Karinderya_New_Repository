using UnityEngine;

public enum CustomerType
{
    Boy,
    Girl,
    OldMan,
    OldWoman
}

public class CustomerAppearance : MonoBehaviour
{
    public CustomerType customerType;
    public Color tintColor = Color.white;

    Renderer[] characterRenderers;
    MaterialPropertyBlock block;

    void Awake()
    {
        CacheRenderers();
    }

    public void Apply(CustomerType type)
    {
        customerType = type;
        tintColor = GetTint(type);

        CacheRenderers();
        ApplyTint();
    }

    void CacheRenderers()
    {

        Transform visual = transform.Find("npc");

        if (visual != null)
        {
            characterRenderers =
                visual.GetComponentsInChildren<Renderer>(true);

            return;
        }

        characterRenderers =
            GetComponentsInChildren<SkinnedMeshRenderer>(true);
    }

    void ApplyTint()
    {
        if (characterRenderers == null ||
            characterRenderers.Length == 0)
        {
            return;
        }

        if (block == null)
            block = new MaterialPropertyBlock();

        foreach (Renderer renderer in characterRenderers)
        {
            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(block);

            block.SetColor("_BaseColor", tintColor);
            block.SetColor("_Color", tintColor);

            renderer.SetPropertyBlock(block);
        }
    }

    public static Color GetTint(CustomerType type)
    {
        switch (type)
        {
            case CustomerType.Boy:
                return new Color(0.55f, 0.78f, 1f, 1f);

            case CustomerType.Girl:
                return new Color(1f, 0.62f, 0.78f, 1f);

            case CustomerType.OldMan:
                return new Color(0.72f, 0.76f, 0.82f, 1f);

            case CustomerType.OldWoman:
                return new Color(0.82f, 0.68f, 0.92f, 1f);
        }

        return Color.white;
    }

    public static CustomerType RollType()
    {

        int roll = Random.Range(0, 100);

        if (roll < 30)
            return CustomerType.Boy;

        if (roll < 60)
            return CustomerType.Girl;

        if (roll < 80)
            return CustomerType.OldMan;

        return CustomerType.OldWoman;
    }
}
