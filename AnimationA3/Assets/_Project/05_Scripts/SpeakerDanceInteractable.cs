using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class SpeakerDanceInteractable : BaseInteractable
{
    [Header("Prompt Images")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private GameObject pressDanceImage;
    [SerializeField] private GameObject pressStopImage;

    private PlayerController _player;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        HidePrompt();
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
        UpdatePromptImages();
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

        UpdatePromptImages();
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
        HidePrompt();
    }

    private void UpdatePromptImages()
    {
        bool isDancing =
            _player != null && _player.IsDancing;

        if (pressDanceImage != null)
        {
            pressDanceImage.SetActive(!isDancing);
        }

        if (pressStopImage != null)
        {
            pressStopImage.SetActive(isDancing);
        }
    }

    private void HidePrompt()
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }

        if (pressDanceImage != null)
        {
            pressDanceImage.SetActive(false);
        }

        if (pressStopImage != null)
        {
            pressStopImage.SetActive(false);
        }
    }
}