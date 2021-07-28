using UnityEngine;

public class a_MoveNotifButton : MonoBehaviour
{
    public void ShowButton()
    {
        transform.localPosition = new Vector2(0, 0);
        StartCoroutine(gameObject.MoveLocalY(-140, 0.5f));
    }

    public void HideButton()
    {
        StartCoroutine(gameObject.MoveLocalY(0, 0.5f));
    }

}
