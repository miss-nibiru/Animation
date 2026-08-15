using UnityEngine;
using UnityEngine.Playables;

public class CinematicGameplayHandoff : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector timelineDirector;

    [Header("Characters")]
    [SerializeField] private GameObject cinematicCharacter;
    [SerializeField] private GameObject gameplayCharacter;

    [Header("Gameplay Starting Position")]
    [SerializeField] private Transform gameplayStartPoint;

    private void OnEnable()
    {
        timelineDirector.stopped += BeginGameplay;
    }

    private void OnDisable()
    {
        timelineDirector.stopped -= BeginGameplay;
    }

    private void Start()
    {
        cinematicCharacter.SetActive(true);
        gameplayCharacter.SetActive(false);

        timelineDirector.time = 0;
        timelineDirector.Evaluate();
        timelineDirector.Play();
    }

    private void BeginGameplay(PlayableDirector director)
    {
        gameplayCharacter.transform.SetPositionAndRotation(
            gameplayStartPoint.position,
            gameplayStartPoint.rotation
        );

        cinematicCharacter.SetActive(false);
        gameplayCharacter.SetActive(true);
    }
}