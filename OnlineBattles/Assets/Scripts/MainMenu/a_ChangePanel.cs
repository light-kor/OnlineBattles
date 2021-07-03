using System.Collections;
using UnityEngine;

public class a_ChangePanel : MonoBehaviour
{
    private Kino.AnalogGlitch _glitch;
    private float AnimTime = 0;

    private void Start()
    {
        MainMenu.ChangePanel += Change;
        _glitch = Camera.main.gameObject.GetComponent<Kino.AnalogGlitch>();
        AnimTime = MainMenu.AnimTime;
    }

    private void Change()
    {
        Debug.Log("wwww");
        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        float time = 0f;
        while (time < AnimTime)
        {
            time += Time.deltaTime;
            _glitch.verticalJump += Time.deltaTime / AnimTime;
            yield return null;
        }

        time = 0f;
        while (time < AnimTime)
        {
            time += Time.deltaTime;
            _glitch.verticalJump -= Time.deltaTime / AnimTime;
            yield return null;
        }

        _glitch.verticalJump = 0.01f;
    }
}
