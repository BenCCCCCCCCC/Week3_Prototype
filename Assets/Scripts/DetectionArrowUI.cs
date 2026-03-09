using UnityEngine;

public class DetectionArrowUI : MonoBehaviour
{
    public RectTransform arrowRect;
    public float screenEdgePadding = 120f;

    private Transform target;
    private Camera targetCamera;

    void Awake()
    {
        if (arrowRect == null)
        {
            arrowRect = GetComponent<RectTransform>();
        }

        gameObject.SetActive(false);
    }

    public void SetTarget(Transform newTarget, Camera cam)
    {
        target = newTarget;
        targetCamera = cam;
        gameObject.SetActive(target != null && targetCamera != null);
    }

    public void ClearTarget()
    {
        target = null;
        targetCamera = null;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (target == null || targetCamera == null || arrowRect == null)
        {
            gameObject.SetActive(false);
            return;
        }

        RectTransform parentRect = arrowRect.parent as RectTransform;
        if (parentRect == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 viewportPos = targetCamera.WorldToViewportPoint(target.position);
        Vector2 dir = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);

        if (viewportPos.z < 0f)
        {
            dir = -dir;
        }

        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = Vector2.up;
        }

        dir.Normalize();

        Vector2 halfSize = parentRect.rect.size * 0.5f - new Vector2(screenEdgePadding, screenEdgePadding);
        arrowRect.anchoredPosition = new Vector2(dir.x * halfSize.x, dir.y * halfSize.y);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}