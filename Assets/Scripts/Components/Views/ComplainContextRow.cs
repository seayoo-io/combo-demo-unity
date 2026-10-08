using System;
using UnityEngine;
using UnityEngine.UI;

internal class ComplainContextRow : MonoBehaviour
{
    public InputField keyInput;
    public Dropdown typeDropdown;
    public InputField valueInput;
    public Dropdown booleanDropdown;
    public Button removeButton;
    private Action<ComplainContextRow> remove;

    public void Initialize(Action<ComplainContextRow> onRemove)
    {
        remove = onRemove;
        typeDropdown.onValueChanged.AddListener(OnTypeChanged);
        removeButton.onClick.AddListener(Remove);
        typeDropdown.value = (int)ComplainContextType.String;
        OnTypeChanged(typeDropdown.value);
    }

    public ComplainContextEntry ReadEntry()
    {
        var type = (ComplainContextType)typeDropdown.value;
        return new ComplainContextEntry
        {
            Key = keyInput.text,
            Type = type,
            Value = type == ComplainContextType.Boolean
                ? (booleanDropdown.value == 0 ? "true" : "false")
                : valueInput.text
        };
    }

    private void OnTypeChanged(int index)
    {
        var isBoolean = index == (int)ComplainContextType.Boolean;
        valueInput.gameObject.SetActive(!isBoolean);
        booleanDropdown.gameObject.SetActive(isBoolean);
    }

    private void Remove() => remove?.Invoke(this);

    private void OnDestroy()
    {
        typeDropdown.onValueChanged.RemoveListener(OnTypeChanged);
        removeButton.onClick.RemoveListener(Remove);
    }
}
