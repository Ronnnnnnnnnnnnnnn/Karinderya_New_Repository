using UnityEngine;
using TMPro;

public class CropFloatingUI : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    public TextMeshProUGUI text;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        GameObject prefab = Resources.Load<GameObject>("CropTimerUI");
        Debug.Log("Prefab = " + prefab);
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + offset;

        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }
    }

    public void SetText(string value)
    {
        if (text != null)
            text.text = value;
    }
}