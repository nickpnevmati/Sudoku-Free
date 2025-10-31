using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using deVoid.Utils;
using System.Data.Common;

public class NumpadController : MonoBehaviour
{
    private Button[] buttons = new Button[9];
    private Toggle[] toggles = new Toggle[9];

    private Toggle activeToggle;

    public Action<int> onButtonClicked;

    private bool toggleMode;

    private List<int> disabled = new List<int>();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            int childIndex = int.Parse(child.name.Split('.')[1]);
            Button button = child.GetComponentInChildren<Button>();
            button.onClick.AddListener(delegate { onButtonClicked.Invoke(childIndex); });
            buttons[childIndex - 1] = button;

            foreach (var tmp_text in child.GetComponentsInChildren<TMP_Text>(includeInactive: true))
                tmp_text.text = childIndex.ToString();

            Toggle toggle = child.GetComponentInChildren<Toggle>(includeInactive: true);
            toggle.onValueChanged.AddListener(delegate(bool value) { TogglePressed(value, childIndex - 1); });
            toggles[childIndex - 1] = toggle;
        }

        Signals.Get<OnSettingsChangedSignal>().AddListener(OnSettingsChanged);
    }

    public void SetToggleMode(bool value)
    {
        toggleMode = value;
        if (!toggleMode)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                TogglePressedWithoutNotify(false, i);
            }
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            if (disabled.Contains(i + 1)) continue;

            buttons[i].gameObject.SetActive(!toggleMode);
            toggles[i].gameObject.SetActive(toggleMode);
        }
    }

    public void DisableNumber(int number)
    {
        disabled.Add(number);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].enabled = false;
            toggles[i].enabled = false;
        }
    }

    public void SetActiveToggle(int number)
    {
        if (!toggleMode) return;
        TogglePressedWithoutNotify(true, number - 1);
    }

    private void TogglePressed(bool newValue, int index)
    {
        TogglePressedWithoutNotify(newValue, index);
        onButtonClicked.Invoke(index + 1);
    }

    private void TogglePressedWithoutNotify(bool newValue, int index)
    {
        foreach (var toggle in toggles)
        {
            toggle.interactable = true;
            toggle.SetIsOnWithoutNotify(false);
        }

        toggles[index].SetIsOnWithoutNotify(newValue);
        toggles[index].interactable = !newValue;
    }

    private void OnSettingsChanged(Settings settings)
    {

    }
}
