using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UIElements.Experimental;

[RequireComponent(typeof(Toggle))]
public class SliderToggle : MonoBehaviour
{
    [SerializeField] string labelText;
    [SerializeField] TMP_Text label;

    [SerializeField] Image slider;
    [SerializeField] Image sliderBackground;

    [SerializeField] RectTransform sliderOffPos, sliderOnPos;
    [SerializeField] float transitionTime;

    private Toggle toggle;

    void Awake()
    {
        label.text = labelText;

        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(TriggerToggleAnimation);

        SetState(toggle.isOn);
    }

    public void SetState(bool value)
    {
        toggle.SetIsOnWithoutNotify(true);
        slider.transform.position = toggle.isOn ? sliderOnPos.position : sliderOffPos.position;
    }

    private void TriggerToggleAnimation(bool value)
    {
        StopAllCoroutines();
        StartCoroutine(ToggleAnimation(value));
    }

    private IEnumerator ToggleAnimation(bool value)
    {
        RectTransform origin = value ? sliderOffPos : sliderOnPos;
        Vector3 originAnchored = origin.position;

        RectTransform target = value ? sliderOnPos : sliderOffPos;
        Vector3 targetAnchored = target.position;

        float elapsedTime = 0f;
        while (elapsedTime < transitionTime)
        {
            float percent = elapsedTime / transitionTime;
            Vector3 currentOffset = (targetAnchored - originAnchored) * percent;
            elapsedTime += Time.deltaTime;

            slider.transform.position = origin.position + currentOffset;

            yield return null;
        }

        slider.transform.position = target.position;
    }

    private void OnValidate()
    {
        label.text = labelText;
    }
}
