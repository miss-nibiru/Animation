using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class SpeakerDanceInteractable : BaseInteractable
{
    [Header("Prompt")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;

    [SerializeField] private string startDancePrompt =
        "Press E to PARTAY!";

    [SerializeField] private string stopDancePrompt =
        "Press E to STAHP!";

    private PlayerController _player;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (_player == null)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        _player.ToggleDance();
        UpdatePrompt();
    }

    public override bool CanInteract()
    {
        return _player != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
        }

        _player = player;

        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
        }

        UpdatePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null || player != _player)
        {
            return;
        }

        if (_player.IsDancing)
        {
            _player.ToggleDance();
        }

        _player = null;

        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
    }

    private void UpdatePrompt()
    {
        if (_player == null || promptText == null)
        {
            return;
        }

        promptText.text =
            _player.IsDancing
                ? stopDancePrompt
                : startDancePrompt;
    }
}