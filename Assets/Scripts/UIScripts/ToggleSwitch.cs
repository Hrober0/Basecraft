using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class ToggleSwitch : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private bool _isOn = false;
    [SerializeField] private bool isInteractible = true;
    public bool isOn { get { return _isOn; } }
    [SerializeField] private RectTransform toggleIndicator = null;
    private Image toggleColor = null;
    [SerializeField] private Image backgroundImage = null;
    public Color onColor;
    public Color offColor;
    public Color disactiveColor;
    [SerializeField] private float tweenTime = 0.25f;

    private float onX = 21f;
    private float offX = -21f;

    public AudioSource audioSource;


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractible) { return; }
        Toggle(!isOn, true);
    }

    //public delegate void ValueChanged(bool value);
    //public event ValueChanged valueChanged;

    private void OnEnable()
    {
        if (TryGetComponent(out Toggle toggle))
        {
            Toggle(toggle.isOn, false);
            SetInteractible(toggle.interactable); ;
        }
        else
        {
            Toggle(isOn, false);
            SetInteractible(isInteractible);
        }
    }
    private void Awake()
    {
        Toggle(isOn, false);
        toggleColor = toggleIndicator.GetComponent<Image>();
    }

    public void Toggle(bool value, bool playAnimation, bool playSound = false)
    {
        _isOn = value;

        ToggleColor(isOn, playAnimation);
        MoveIndicator(isOn, playAnimation);

        if (playSound) { audioSource.Play(); }

        //if (valueChanged != null) { valueChanged(isOn); }
    }

    private void ToggleColor(bool value, bool playAnimation)
    {
        if (playAnimation)
        {
            if (value) { backgroundImage.DOColor(onColor, tweenTime).SetUpdate(true); }
            else { backgroundImage.DOColor(offColor, tweenTime).SetUpdate(true); }
        }
        else
        {
            if (value) { backgroundImage.color = onColor; }
            else { backgroundImage.color = offColor; }
        }
    }
    private void MoveIndicator(bool value, bool playAnimation)
    {
        if (playAnimation)
        {
            if (value) { toggleIndicator.DOAnchorPosX(onX, tweenTime).SetUpdate(true); }
            else { toggleIndicator.DOAnchorPosX(offX, tweenTime).SetUpdate(true); }
        }
        else
        {
            if (value) { toggleIndicator.anchoredPosition = new Vector2(onX, toggleIndicator.anchoredPosition.y); }
            else { toggleIndicator.anchoredPosition = new Vector2(offX, toggleIndicator.anchoredPosition.y); }
        }
        
    }

    private void SetInteractible(bool v)
    {
        isInteractible = v;
        if (v) { toggleColor.color = Color.white; ; }
        else { toggleColor.color = disactiveColor; }
    }
}
