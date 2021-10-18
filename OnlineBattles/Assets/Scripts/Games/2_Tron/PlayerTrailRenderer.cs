using UnityEngine;

namespace Game2
{
    [RequireComponent(typeof(TrailRenderer))]
    public class PlayerTrailRenderer : MonoBehaviour
    {
        private const float TrailTime = 3.2f;
        private TrailRenderer _trail;
        private GameResources_2 GR;
        
        private void Start()
        {
            _trail = GetComponent<TrailRenderer>();
            GR = GameResources_2.GameResources;
            GR.PauseTheGame += StopTrail;
            GR.ResumeTheGame += ResumeTrail;
            GR.StartTheGame += StartTrail;
        }

        public void ClearTrail()
        {
            _trail.emitting = false;
            _trail.time = 0f;
            _trail.Clear();           
        }

        private void StartTrail()
        {
            _trail.time = TrailTime;
            _trail.emitting = true;
        }

        private void StopTrail()
        {
            _trail.time = Mathf.Infinity;
        }

        private void ResumeTrail()
        {
            _trail.time = TrailTime;
        }

        private void OnDestroy()
        {
            GR.PauseTheGame -= StopTrail;
            GR.ResumeTheGame -= ResumeTrail;
        }
    }
}
