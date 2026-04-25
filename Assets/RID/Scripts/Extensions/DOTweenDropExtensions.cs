using UnityEngine;
using DG.Tweening;

public static class DOTweenDropExtensions
{
    /// <summary>
    /// Creates a "drop and wiggle settle" animation for an object, ending at a target rotation.
    /// </summary>
    /// <param name="target">The transform to animate.</param>
    /// <param name="finalRotation">Target world-space rotation (Euler angles).</param>
    /// <param name="duration">Total duration of the full animation.</param>
    /// <param name="amplitude">Maximum wiggle amplitude in degrees.</param>
    /// <param name="vibrations">Number of wiggle oscillations before settling.</param>
    /// <returns>A DOTween Sequence.</returns>
    public static Sequence DODropWiggle(
        this Transform target,
        Vector3 finalRotation,
        float duration = 1f,
        float amplitude = 10f,
        int vibrations = 4
    )
    {
        Sequence seq = DOTween.Sequence();

        // Split the total duration proportionally
        float dropTime = duration * 0.25f;
        float wiggleTime = duration * 0.55f;
        float settleTime = duration * 0.2f;

        // Start with small random offset (like falling rotation)
        Vector3 startRot = finalRotation + new Vector3(
            Random.Range(-amplitude * 1.5f, amplitude * 1.5f),
            Random.Range(-amplitude * 0.5f, amplitude * 0.5f),
            Random.Range(-amplitude, amplitude)
        );

        target.rotation = Quaternion.Euler(startRot);

        // Step 1: Snap-drop to target
        seq.Append(target.DORotate(finalRotation, dropTime)
            .SetEase(Ease.OutQuad));

        // Step 2: Wiggle (like a spring vibration)
        for (int i = 0; i < vibrations; i++)
        {
            float t = (i / (float)vibrations);
            float strength = Mathf.Lerp(amplitude, 0f, t);
            float halfTime = wiggleTime / (vibrations * 2f);

            seq.Append(target.DORotate(finalRotation + new Vector3(0, 0, strength), halfTime)
                .SetEase(Ease.OutQuad));
            seq.Append(target.DORotate(finalRotation - new Vector3(0, 0, strength * 0.7f), halfTime)
                .SetEase(Ease.OutQuad));
        }

        // Step 3: Final settle
        seq.Append(target.DORotate(finalRotation, settleTime)
            .SetEase(Ease.OutCubic));

        return seq;
    }
    
    /// <summary>
    /// Creates a "drop and wiggle settle" animation for an object, using local rotation space.
    /// </summary>
    /// <param name="target">Transform to animate (local rotation).</param>
    /// <param name="finalLocalRotation">Target local-space rotation (Euler angles).</param>
    /// <param name="duration">Total duration of the full animation.</param>
    /// <param name="amplitude">Maximum wiggle amplitude in degrees.</param>
    /// <param name="vibrations">Number of wiggle oscillations before settling.</param>
    /// <returns>A DOTween Sequence.</returns>
    public static Sequence DOLocalDropWiggle(
        this Transform target,
        Vector3 finalLocalRotation,
        float duration = 1f,
        float amplitude = 10f,
        int vibrations = 4
    )
    {
        Sequence seq = DOTween.Sequence();

        // Timing split
        float dropTime = duration * 0.25f;
        float wiggleTime = duration * 0.55f;
        float settleTime = duration * 0.2f;

        // Start with random local offset (simulating falling rotation)
        Vector3 startRot = finalLocalRotation + new Vector3(
            Random.Range(-amplitude * 1.5f, amplitude * 1.5f),
            Random.Range(-amplitude * 0.5f, amplitude * 0.5f),
            Random.Range(-amplitude, amplitude)
        );

        target.localEulerAngles = startRot;

        // Step 1: Snap-drop to target
        seq.Append(target.DOLocalRotate(finalLocalRotation, dropTime)
            .SetEase(Ease.OutQuad));

        // Step 2: Wiggle like spring oscillation
        for (int i = 0; i < vibrations; i++)
        {
            float t = i / (float)vibrations;
            float strength = Mathf.Lerp(amplitude, 0f, t);
            float halfTime = wiggleTime / (vibrations * 2f);

            seq.Append(target.DOLocalRotate(finalLocalRotation + new Vector3(0, 0, strength), halfTime)
                .SetEase(Ease.OutQuad));
            seq.Append(target.DOLocalRotate(finalLocalRotation - new Vector3(0, 0, strength * 0.7f), halfTime)
                .SetEase(Ease.OutQuad));
        }

        // Step 3: Final settle
        seq.Append(target.DOLocalRotate(finalLocalRotation, settleTime)
            .SetEase(Ease.OutCubic));

        return seq;
    }
}
