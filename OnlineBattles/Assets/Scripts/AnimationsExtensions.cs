using System;
using System.Collections;
using UnityEngine;

public static class AnimationsExtensions
{
    public static float EssingSmoothSquared(this float x)
    {
        return x < 0.5 ? x * x * 2 : (1 - (x - 1) * (1 - x) * 2);
    }

    public static float EssingInverseSquared(this float x)
    {
        return 1 - (1 - x) * (1 - x);
    }

    public static IEnumerator Moving(this GameObject obj, Vector3 startPos, Vector3 targetPos)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            obj.transform.position = Vector3.Lerp(startPos, targetPos, timer.EssingInverseSquared());
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public static IEnumerator Moving(this GameObject obj, Vector3 targetPos, float time)
    {
        float timer = 0f;
        Vector3 startPos = obj.transform.position;

        while (timer < 1f)
        {
            obj.transform.position = Vector3.Lerp(startPos, targetPos, timer.EssingInverseSquared());
            timer += Time.deltaTime / time;
            yield return null;
        }
    }

    public static IEnumerator MoveLocalY(this GameObject obj, float targetPosY, float time)
    {
        float timer = 0f;
        Vector3 startPos = obj.transform.localPosition;
        Vector3 targetPos = new Vector2(obj.transform.localPosition.x, targetPosY);

        while (timer < 1f)
        {
            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, timer.EssingInverseSquared());
            timer += Time.deltaTime / time;
            yield return null;
        }
    }

    public static IEnumerator MoveLocalY(this GameObject obj, float targetPosY, float time, Action setOnComplete)
    {
        float timer = 0f;
        Vector3 startPos = obj.transform.localPosition;
        Vector3 targetPos = new Vector2(obj.transform.localPosition.x, targetPosY);

        while (timer < 1f)
        {
            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, timer.EssingInverseSquared());
            timer += Time.deltaTime / time;
            yield return null;
        }

        setOnComplete();
    }

}
