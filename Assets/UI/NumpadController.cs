using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class NumpadController : MonoBehaviour
{
    private Button[] buttons = new Button[9];
    private Toggle[] toggles = new Toggle[9];

    private Toggle activeToggle;

    public Action<int?> onButtonClicked;

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
            toggle.onValueChanged.AddListener(TogglePressed);
            toggles[childIndex - 1] = toggle;
        }
    }

    public void SetModeToggle(bool value)
    {
        toggleMode = value;
        if (!toggleMode)
        {
            onButtonClicked.Invoke(null);
            if (activeToggle) activeToggle.isOn = false;
            activeToggle = null;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            if (disabled.Contains(i + 1)) continue;

            buttons[i].gameObject.SetActive(!toggleMode);
            toggles[i].gameObject.SetActive(toggleMode);
        }

        var idx = Enumerable.Range(0, toggles.Length).FirstOrDefault(i => !disabled.Contains(i + 1));
        if (idx < toggles.Length)
        {
            toggles[idx].isOn = true;
            onButtonClicked.Invoke(idx + 1);
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

        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = false;
        }
        toggles[number - 1].isOn = true;
    }

    private void TogglePressed(bool newValue)
    {
        if (!newValue)
        {
            onButtonClicked.Invoke(null);
            activeToggle = null;
            return;
        }

        if (activeToggle) activeToggle.isOn = false;

        //assuming at-most one toggle is on at a time, this should work
        int index = Array.FindIndex(toggles, x => x.isOn);
        activeToggle = toggles[index];
        onButtonClicked.Invoke(index + 1);
    }
}
