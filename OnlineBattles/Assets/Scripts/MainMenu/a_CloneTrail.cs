using UnityEngine;

public class a_CloneTrail : MonoBehaviour
{
    [SerializeField] private GameObject _echo, _canvas;
    [SerializeField] private float _startTimeBtwSpawns;
    private float _timeBtwSpawns;

    private void Update()
    {
        if (_timeBtwSpawns <= 0)
        {
            GameObject instance = Instantiate(_echo, transform.position, Quaternion.identity, _canvas.transform);
            Destroy(instance, 1f);
            _timeBtwSpawns = _startTimeBtwSpawns;
        }
        else
            _timeBtwSpawns -= Time.deltaTime;
    }
}
