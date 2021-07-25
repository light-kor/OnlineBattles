using UnityEngine;

public class a_MoveNotifButton : MonoBehaviour
{
    public void ShowButton()
    {
        transform.localPosition = new Vector2(0, 0);
        transform.LeanMoveLocalY(-140, 0.5f).setEaseOutExpo().delay = 0.1f;
    }

    public void HideButton()
    {
        transform.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
    }

}
