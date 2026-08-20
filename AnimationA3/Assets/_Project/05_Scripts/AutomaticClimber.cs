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
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oneShot && _hasTriggered)
        {
            return;
        }

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
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
    }
}