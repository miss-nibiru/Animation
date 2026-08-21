using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider))]
public class EndingCinematicTrigger : MonoBehaviour
{
    [Header("Ending Timeline")]
    [SerializeField] private PlayableDirector endingDirector;

    [Header("Michelle")]
    [SerializeField] private GameObject gameplayMichelle;
    [SerializeField] private GameObject cinematicMichelle;

    private bool hasTriggered;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (cinematicMichelle != null)
        {
            cinematicMichelle.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        StartEnding();
    }

    private void StartEnding()
    {
        hasTriggered = true;

        if (gameplayMichelle != null)
        {
            gameplayMichelle.SetActive(false);
        }

        if (cinematicMichelle != null)
        {
            cinematicMichelle.SetActive(true);
        }

        endingDirector.time = 0;
        endingDirector.Play();
    }
}