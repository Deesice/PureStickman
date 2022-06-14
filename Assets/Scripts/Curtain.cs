using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Curtain : MonoBehaviour
{
    public const float DefaultTime = 1;
    [SerializeField] GameObject curtainCastBlock;
    [SerializeField] Image[] curtainPieces;
    [SerializeField] CanvasScaler canvasScaler;
    Vector3[] closePositions;
    Vector3[] openPositions;
    public bool Opened { get; private set; }
    public static Curtain Instance { get; private set; }
    static Color? color;
    AudioSource source;
    [SerializeField] Sound whooshSound;
    public event Action OpeningCurtainDone;
    void Awake()
    {
        source = GetComponent<AudioSource>();
        foreach (var i in curtainPieces)
            i.enabled = true;

        if (color == null)
        {
            SetColor(Color.black);
        }
        else
        {
            SetColor((Color)color);
        }
        Instance = this;
        closePositions = new Vector3[curtainPieces.Length];
        openPositions = new Vector3[curtainPieces.Length];
        for (int i = 0; i < curtainPieces.Length; i++)
        {
            var piece = curtainPieces[i].rectTransform;
            closePositions[i] = piece.position;
            openPositions[i] = piece.position - piece.up * piece.rect.height * canvasScaler.GetRelative4K();
        }
    }
    public void SetColor(Color color)
    {
        Curtain.color = color;
        foreach (var c in curtainPieces)
            c.color = color;
    }
    private void Start()
    {
        Open(DefaultTime, () => OpeningCurtainDone?.Invoke());
    }
    [ContextMenu("Open")]
    void Open()
    {
        Open(DefaultTime);
    }
    public void Open(float time, Action callback = null)
    {
        if (Opened)
            return;

        Opened = true;
        if (time > 0)
        {
            StartCoroutine(ChangingState(time, true, callback));
        }
        else
        {
            curtainCastBlock?.SetActive(false);
            for (int i = 0; i < curtainPieces.Length; i++)
            {
                curtainPieces[i].rectTransform.position = openPositions[i];
            }
        }
    }
    [ContextMenu("Close")]
    void Close()
    {
        Close(DefaultTime);
    }
    public void Close(float time, Action callback = null)
    {
        if (!Opened)
            return;

        curtainCastBlock?.SetActive(true);
        Opened = false;
        if (time > 0)
        {
            StartCoroutine(ChangingState(time, false, callback));
        }
        else
        {
            for (int i = 0; i < curtainPieces.Length; i++)
            {
                curtainPieces[i].rectTransform.position = closePositions[i];
            }
        }
    }
    IEnumerator ChangingState(float totalAnimationTime, bool open, Action callback)
    {
        float timeForOnePiece = totalAnimationTime / curtainPieces.Length;
        for (int j = 0; j < curtainPieces.Length; j++)
        {
            whooshSound.Play(source, Mathf.InverseLerp(1, 0.4f, totalAnimationTime));
            float i = 0;
            var startPos = !open ? openPositions[j] : closePositions[j];
            var endPos = open ? openPositions[j] : closePositions[j];
            while (i < 1)
            {
                yield return null;
                i += Time.deltaTime / timeForOnePiece;
                curtainPieces[j].rectTransform.position = Vector3.Lerp(startPos, endPos, i);
            }
        }
        callback?.Invoke();
        if (open)
            curtainCastBlock?.SetActive(false);
    }
}
