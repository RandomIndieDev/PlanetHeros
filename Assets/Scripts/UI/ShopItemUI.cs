using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image m_ShopItemImage;
    public GameObject m_PurchaseButton;
    public Button m_EquipButton;

    public void Clear()
    {
        gameObject.SetActive(false);

        if (m_ShopItemImage != null)
        {
            m_ShopItemImage.sprite = null;
            m_ShopItemImage.enabled = false;
        }

        if (m_PurchaseButton != null)
        {
            m_PurchaseButton.SetActive(false);
        }

        if (m_EquipButton != null)
        {
            m_EquipButton.onClick.RemoveAllListeners();
            m_EquipButton.gameObject.SetActive(false);
        }
    }

    public void Setup(Sprite icon, bool showPurchaseButton, bool showEquipButton, UnityEngine.Events.UnityAction onEquip)
    {
        gameObject.SetActive(true);

        if (m_ShopItemImage != null)
        {
            m_ShopItemImage.enabled = icon != null;
            m_ShopItemImage.sprite = icon;
        }

        if (m_PurchaseButton != null)
        {
            m_PurchaseButton.SetActive(showPurchaseButton);
        }

        if (m_EquipButton != null)
        {
            m_EquipButton.onClick.RemoveAllListeners();
            m_EquipButton.gameObject.SetActive(showEquipButton);

            if (showEquipButton && onEquip != null)
            {
                m_EquipButton.onClick.AddListener(onEquip);
            }
        }
    }
}
