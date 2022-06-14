using UnityEngine;
using System;
using UnityEngine.UI;

public class CharacterLibrary : MonoBehaviour
{
    [SerializeField] float whooshValue;
    [SerializeField] Sound whooshSound;
    [SerializeField] GameObject buyParticle;
    [SerializeField] Item[] characterPresenters;
    [SerializeField] int selectedCharacter;
    [SerializeField] float circleRadius;
    [SerializeField] float circleWidthMul;
    [SerializeField] float switchSpeed;
    [SerializeField] float fingerSensitivity;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float centerOffset;
    AudioSource source;
    float lastWhooshTime;
    Camera cam;
    Transform[] characters;
    bool rotateCharacter;
    private void Awake()
    {
        cam = Camera.main;
        characters = new Transform[characterPresenters.Length];
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i] = Instantiate(characterPresenters[i].prefab).transform;
            characters[i].GetComponent<IKHelper>().enabled = false;
            characters[i].GetComponent<Player>().enabled = false;
        }
    }
    private void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        Inventory.Instance.Equipped += OnEquipped;

        OnEquipped(Inventory.Instance.GetEquippedItem(Item.ItemSubType.Character));
    }
    private void OnDestroy()
    {
        Inventory.Instance.Equipped -= OnEquipped;
    }
    void OnEquipped(Item item)
    {
        for (int i = 0; i < characterPresenters.Length; i++)
        {
            if (characterPresenters[i] == item)
            {
                if (selectedCharacter != i)
                {
                    whooshSound.Play(source, whooshValue / (Time.time - lastWhooshTime));
                    lastWhooshTime = Time.time;
                }
                selectedCharacter = i;
                return;
            }
        }
    }
    void Update()
    {
        Vector3 center = transform.position - transform.right * circleRadius;
        center += cam.transform.right * cam.aspect * centerOffset;
        var angleStep = Mathf.PI * 2 / characters.Length;
        for (int i = 0; i < characters.Length; i++)
        {
            var deltaAngle = (i - selectedCharacter) * angleStep;
            var wantedPosition = new Vector3(Mathf.Cos(deltaAngle) * circleRadius, 0, Mathf.Sin(deltaAngle) * circleWidthMul * circleRadius);
            wantedPosition += center;
            characters[i].position = Vector3.Lerp(characters[i].position, wantedPosition, Time.deltaTime * switchSpeed);
        }

        if (Input.GetMouseButtonDown(0))
            rotateCharacter = cam.ScreenToViewportPoint(Input.mousePosition).x >= 0.5f;

        if (rotateCharacter)
        {            
            var current = characters[selectedCharacter];
            var value = Input.mousePresent ? Input.GetAxis("Mouse X") * mouseSensitivity : Input.GetTouch(0).deltaPosition.x * fingerSensitivity / Screen.dpi;
            current.Rotate(Vector3.down * value * Time.deltaTime, Space.Self);
        }
    }
}
