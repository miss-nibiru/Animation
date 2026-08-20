using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AutomaticClimbTrigger : MonoBehaviour
{
    [Header("Climb Points")]
    [SerializeField] private Transform climbStart;
    [SerializeField] private Transform climbEnd;

    [Header("Settings")]
    [SerializeField] private bool oneShot = true;

    private bool _hasTriggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
        }

        player.EnterClimbTrigger(this);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
        }

        player.ExitClimbTrigger(this);
    }

    public bool TryStartClimb(PlayerController player)
    {
        if (player == null)
        {
            return false;
        }

        if (oneShot && _hasTriggered)
        {
            return false;
        }

        if (climbStart == null || climbEnd == null)
        {
            return false;
        }

        bool started =
            player.BeginAutomaticClimb(
                climbStart,
                climbEnd
            );

        if (started)
        {
            _hasTriggered = true;
        }

        return started;
    }
}
