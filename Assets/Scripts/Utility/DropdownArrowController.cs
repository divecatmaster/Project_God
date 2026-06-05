using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownArrowController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMP_Dropdown Dropdown;
    [SerializeField] RectTransform Arrow;

    private bool _isOpen;

    private void Awake()
    {
        if (Dropdown != null)
            Dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        if (Dropdown != null)
            Dropdown.onValueChanged.RemoveListener(OnValueChanged);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _isOpen = true;
        SetArrow(true);
    }

    private void Update()
    {
        if (!_isOpen || Dropdown == null)
            return;

        bool hasMyDropdownList = false;

        for (int i = 0; i < Dropdown.transform.childCount; i++)
        {
            Transform child = Dropdown.transform.GetChild(i);

            if (child.name.StartsWith("Dropdown List"))
            {
                hasMyDropdownList = true;
                break;
            }
        }

        if (!hasMyDropdownList)
        {
            _isOpen = false;
            SetArrow(false);
        }
    }

    private void OnValueChanged(int index)
    {
        _isOpen = false;
        SetArrow(false);
    }

    private void OnDisable()
    {
        _isOpen = false;
        SetArrow(false);
    }

    private void SetArrow(bool open)
    {
        if (Arrow == null)
            return;

        Arrow.localRotation = open
            ? Quaternion.Euler(0, 0, 180)
            : Quaternion.identity;
    }
}