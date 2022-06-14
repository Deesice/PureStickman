using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassView : MonoBehaviour
{
    public RectTransform rectTransform => backImage.rectTransform;
    [SerializeField] Image coloredImage;
    Image backImage;
    [SerializeField] Sprite[] coloredForms;
    [SerializeField] Sprite[] backForms;
    Text text;
    int lastPivotIdx = -1;
    MaskableGraphic[] maskableGraphics;
    public int compassTypeIdx { get; private set; }
    private void Awake()
    {
        maskableGraphics = GetComponentsInChildren<MaskableGraphic>();
        compassTypeIdx = -1;
        backImage = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
    }
    //private void OnEnable()
    //{
    //    foreach (var i in maskableGraphics)
    //        i.enabled = true;
    //}
    //private void OnDisable()
    //{
    //    foreach (var i in maskableGraphics)
    //        i.enabled = false;
    //}
    public void Setup(string localizedText, Color color, int typeIdx)
    {
        coloredImage.color = color;
        text.text = localizedText;
        compassTypeIdx = typeIdx;
    }
    public void SetPivot(float horizontalViewport)
    {
        var currentPivotIdx = horizontalViewport > 0 ? horizontalViewport < 1 ? 1 : 2 : 0;
        if (currentPivotIdx == lastPivotIdx)
            return;
        lastPivotIdx = currentPivotIdx;
        rectTransform.pivot = new Vector2(lastPivotIdx / 2.0f, rectTransform.pivot.y);
        coloredImage.sprite = coloredForms[lastPivotIdx];
        backImage.sprite = backForms[lastPivotIdx];
    }
}
