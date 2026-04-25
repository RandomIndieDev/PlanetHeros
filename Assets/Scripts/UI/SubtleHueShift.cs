using UnityEngine;
using UnityEngine.UI;

public class SubtleHueShift : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Image m_Image; // or use SpriteRenderer

    [Header("Variation Settings")]
    [Range(0f, 0.1f)] public float m_HueShiftRange = 0.03f;
    [Range(0f, 0.1f)] public float m_SaturationShiftRange = 0.02f;
    [Range(0f, 0.1f)] public float m_ValueShiftRange = 0.02f;

    private void OnEnable()
    {
        ApplyVariation();
    }

    public void ApplyVariation()
    {
        if (m_Image == null || m_Image.sprite == null)
            return;

        Texture2D originalTex = m_Image.sprite.texture;

        // Clone texture
        Texture2D newTex = new Texture2D(originalTex.width, originalTex.height, TextureFormat.RGBA32, false);
        newTex.SetPixels(originalTex.GetPixels());

        float hueOffset = Random.Range(-m_HueShiftRange, m_HueShiftRange);
        float satOffset = Random.Range(-m_SaturationShiftRange, m_SaturationShiftRange);
        float valOffset = Random.Range(-m_ValueShiftRange, m_ValueShiftRange);

        Color[] pixels = newTex.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];

            // skip transparent pixels
            if (c.a < 0.1f)
                continue;

            Color.RGBToHSV(c, out float h, out float s, out float v);

            h = Mathf.Repeat(h + hueOffset, 1f);
            s = Mathf.Clamp01(s + satOffset);
            v = Mathf.Clamp01(v + valOffset);

            Color newColor = Color.HSVToRGB(h, s, v);
            newColor.a = c.a;

            pixels[i] = newColor;
        }

        newTex.SetPixels(pixels);
        newTex.Apply();

        // Create new sprite
        Sprite newSprite = Sprite.Create(
            newTex,
            m_Image.sprite.rect,
            new Vector2(0.5f, 0.5f),
            m_Image.sprite.pixelsPerUnit
        );

        m_Image.sprite = newSprite;
    }
}