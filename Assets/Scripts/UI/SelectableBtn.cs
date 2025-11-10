using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableBtn : MonoBehaviour, IPointerClickHandler
{
    [Header("Images")]
    public Image selectedImage; // Reference to the selected state image

    [Header("Transition Settings")]
    public float fadeTime = 0.2f;
    
    public ButtonTracker buttonTracker;
    
    [Header("Events")]
    public SelectEvent onSelect;

    private void Awake()
    {
        // Ensure the selected image starts fully transparent
        if (selectedImage != null)
        {
            Color startColor = selectedImage.color;
            startColor.a = 0f;
            selectedImage.color = startColor;
        }
    }
    
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        onSelect?.Invoke(this);
    }

    [Button]
    public void Select()
    {
        selectedImage.DOKill();
        selectedImage.DOFade(1f, fadeTime);
        buttonTracker.RecordButtonPress();
    }

    [Button]
    public void UnSelect()
    {
        selectedImage.DOKill();
        selectedImage.DOFade(0f, fadeTime);
    }
}

[System.Serializable]
public class SelectEvent : UnityEvent<SelectableBtn>
{
}