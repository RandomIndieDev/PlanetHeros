using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;
using static Lofelt.NiceVibrations.HapticPatterns;

namespace RID
{
    public class HapticManager : MonoBehaviour, ICoreSystemManager
    {
        private bool m_CanPlayHaptik;

        bool m_IsCooledDown = true;
        
        public void Init()
        {
            
        }

        public void PlayHaptic(PresetType type, bool hasHeavyCanvasOps = false)
        {
            if (m_IsCooledDown && DeviceCapabilities.isVersionSupported)
            {
                PlayPreset(type);
            }
        }

        IEnumerator ClearCoolDownFlag(float time)
        {
            yield return new WaitForSeconds(time);
            m_IsCooledDown = true;
        }
    }
}