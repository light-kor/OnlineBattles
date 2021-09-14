using UnityEngine;

public class a_MoveNotifButton : MonoBehaviour
{
    public bool ButtonActivated { get; private set; } = false;

    public void ShowButton()
    {
        ButtonActivated = true;
        transform.localPosition = new Vector2(0, 0);
        StartCoroutine(gameObject.MoveLocalY(-140, 0.5f));
    }

    public void HideButton()
    {
        ButtonActivated = false;
        StartCoroutine(gameObject.MoveLocalY(0, 0.5f));
    }

}
