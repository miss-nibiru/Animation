using UnityEngine;

public class FixedAngleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothness = 8f;

    private Vector3 _offset;
    private bool _isFollowing;

    public void BeginFollowing()
    {
        _offset = transform.position - target.position;
        _isFollowing = true;
    }

    private void LateUpdate()
    {
        if (!_isFollowing || target == null)
        {
            return;
        }

        Vector3 desiredPosition =
            target.position + _offset;

        float smoothing =
            1f - Mathf.Exp(
                -followSmoothness * Time.deltaTime
            );

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothing
            );
    }
}