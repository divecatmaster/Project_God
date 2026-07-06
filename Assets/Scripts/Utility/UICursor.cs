using UnityEngine;
using UnityEngine.UI;

public class UICursor : MonoBehaviour
{
    [SerializeField] Canvas Canvas;
    [SerializeField] RectTransform CursorRect;
    [SerializeField] Image CursorImage;

    [Header("Setting")]
    [SerializeField] Vector2 HotSpotOffset = Vector2.zero;
    [SerializeField] float CursorSize = 128f;

    private void Awake()
    {
        Cursor.visible = false;

        if (CursorRect != null)
        {
            CursorRect.sizeDelta = new Vector2(CursorSize, CursorSize);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Canvas == null || CursorRect == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Canvas.transform as RectTransform,
            Input.mousePosition,
            Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera,
            out Vector2 localPoint
        );

        CursorRect.anchoredPosition = localPoint + HotSpotOffset;
    }

    public void SetCursorSize(float size)
    {
        CursorSize = size;

        if (CursorRect != null)
        {
            CursorRect.sizeDelta = new Vector2(CursorSize, CursorSize);
        }
    }

    public void SetCursorSprite(Sprite sprite)
    {
        if (CursorImage != null)
        {
            CursorImage.sprite = sprite;
        }
    }
}