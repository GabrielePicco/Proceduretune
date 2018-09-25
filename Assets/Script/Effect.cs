using System.Collections;
using UnityEngine;

public class Effect
{
    public static IEnumerator AnimationScale(GameObject gm, float scTime)
    {
        Vector3 targetScale = gm.transform.localScale;
        Vector3 startingScale = new Vector3(0, 0, targetScale.z);
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / scTime)
        {
            gm.transform.localScale = Vector3.Lerp(startingScale, targetScale, t);
            yield return null;
        }
        gm.transform.localScale = targetScale;
    }

    public static IEnumerator AnimationSlideFromUp(GameObject gm, float scTime)
    {
        Vector3 targetPosition = gm.transform.position;
        Vector3 startingPosition = new Vector3(gm.transform.position.x, gm.transform.position.y + 5, gm.transform.position.z);
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / scTime)
        {
            gm.transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            yield return null;
        }
        gm.transform.position = targetPosition;
    }
}