using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour {

    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _duration = 1f;

    public void ShakeOnce()
    {
        StartCoroutine(Shaking());
    }

    private IEnumerator Shaking() { 
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < _duration) {
            elapsedTime += Time.deltaTime;
            float strength = _curve.Evaluate(elapsedTime / _duration);
            transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
    }
}
