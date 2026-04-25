using UnityEngine;

public class AnimatorOffset : MonoBehaviour
{
    [SerializeField] private Animator[] m_Animators;
    [SerializeField] private float m_OffsetStep = 0.15f;
    [SerializeField] private string m_StateName = "Idle"; // your animation state name

    private void OnEnable()
    {
        ApplyOffsets();
    }

    private void ApplyOffsets()
    {
        for (int i = 0; i < m_Animators.Length; i++)
        {
            Animator anim = m_Animators[i];

            if (anim == null)
                continue;

            anim.enabled = true;

            float offset = (i * m_OffsetStep) % 1f;

            anim.Play(m_StateName, 0, offset);
            anim.Update(0f);
        }
    }
}