using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    [SerializeField]
    private float shakeDuration = 1f;
    [SerializeField] [Range(1f, 100f)]
    private float shakeFrequency = 1f;
    [SerializeField] [Range(0f, 1f)]
    private float shakeScale = 0f;

    private const float maxShake = 5.0f;

    private Coroutine shakeAnimation;

    public void Shake()
    {
        if (shakeAnimation != null)
        {
            StopCoroutine(shakeAnimation);
            shakeAnimation = null;
        }

        shakeAnimation = StartCoroutine(ShakeLoop());
    }

    private IEnumerator ShakeLoop()
    {
        transform.localRotation = Quaternion.identity;

        float time = 0;
        while (time < shakeDuration)
        {
            float Xnoise = shakeScale * maxShake * ((Mathf.PerlinNoise(Time.time * shakeFrequency, 0) - 0.5f) * 2);
            float Ynoise = shakeScale * maxShake * ((Mathf.PerlinNoise(0, Time.time * shakeFrequency) - 0.5f) * 2);
            float Znoise = shakeScale * maxShake * ((Mathf.PerlinNoise(Time.time * shakeFrequency, Time.time * shakeFrequency) - 0.5f) * 2);

            Vector3 noise = new Vector3(Xnoise, Ynoise, Znoise);
            //transform.localPosition = defaultLocalPosition + noise;
            transform.localRotation = Quaternion.identity * Quaternion.Euler(noise);

            time += Time.deltaTime;

            yield return null;
        }

        transform.localRotation = Quaternion.identity;
    }
}
