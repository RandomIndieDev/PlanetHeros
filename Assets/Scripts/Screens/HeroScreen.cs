using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum HeroInventoryCategory
{
    Hats,
    Tops,
    Bottoms,
    Shoes,
    Accessories,
}

[System.Serializable]
public class HeroInventoryItemData
{
    public string ItemId;
    public Sprite ItemIcon;
    public bool IsUnlocked = true;
    public GameObject ClothingObjectToEnable;
}

[System.Serializable]
public class HeroInventoryCategoryData
{
    public HeroInventoryCategory Category;
    public List<HeroInventoryItemData> Items = new List<HeroInventoryItemData>();
}

public class HeroScreen : BaseUIScreen
{
    [SerializeField] private BaseButton m_BackButton;
    [SerializeField] private TextMeshProUGUI m_CurrencyCount;
    [SerializeField] private TextMeshProUGUI m_CategoryTitleText;
    [SerializeField] private BaseButton m_RemoveItemButton;
    [SerializeField] private BaseButton m_Category1Button;
    [SerializeField] private BaseButton m_Category2Button;
    [SerializeField] private BaseButton m_Category3Button;
    [SerializeField] private BaseButton m_Category4Button;
    [SerializeField] private BaseButton m_Category5Button;
    [SerializeField] private List<ShopItemUI> m_ItemUIs = new List<ShopItemUI>();
    [SerializeField] private List<HeroInventoryCategoryData> m_CategoryData = new List<HeroInventoryCategoryData>();

    private readonly Dictionary<HeroInventoryCategory, HeroInventoryCategoryData> m_CategoryLookup = new Dictionary<HeroInventoryCategory, HeroInventoryCategoryData>();
    private readonly Dictionary<HeroInventoryCategory, HeroInventoryItemData> m_EquippedItemsByCategory = new Dictionary<HeroInventoryCategory, HeroInventoryItemData>();
    private readonly Dictionary<HeroInventoryCategory, Image> m_CategoryButtonImages = new Dictionary<HeroInventoryCategory, Image>();
    private readonly Dictionary<HeroInventoryCategory, Color> m_CategoryButtonInitialColors = new Dictionary<HeroInventoryCategory, Color>();
    private HeroInventoryCategory m_CurrentCategory = HeroInventoryCategory.Hats;

    public override void Init(UIManager manager)
    {
        base.Init(manager);
        
        m_BackButton.Subscribe(() =>
        {
            m_ScreenManager.OpenScreen(ScreenName.MainMenu);
        });

        if (m_RemoveItemButton != null)
        {
            m_RemoveItemButton.Subscribe(RemoveEquippedItem);
        }

        SubscribeCategoryButton(m_Category1Button, HeroInventoryCategory.Hats);
        SubscribeCategoryButton(m_Category2Button, HeroInventoryCategory.Tops);
        SubscribeCategoryButton(m_Category3Button, HeroInventoryCategory.Bottoms);
        SubscribeCategoryButton(m_Category4Button, HeroInventoryCategory.Shoes);
        SubscribeCategoryButton(m_Category5Button, HeroInventoryCategory.Accessories);

        CacheCategoryButtonVisual(m_Category1Button, HeroInventoryCategory.Hats);
        CacheCategoryButtonVisual(m_Category2Button, HeroInventoryCategory.Tops);
        CacheCategoryButtonVisual(m_Category3Button, HeroInventoryCategory.Bottoms);
        CacheCategoryButtonVisual(m_Category4Button, HeroInventoryCategory.Shoes);
        CacheCategoryButtonVisual(m_Category5Button, HeroInventoryCategory.Accessories);

        BuildCategoryLookup();
        ClearItemSlots();
    }

    public override void OnOpen(object data = null)
    {
        base.OnOpen(data);
        DailyCurrencyRuntime.CurrencyChanged += RefreshCurrencyText;
        RefreshCurrencyText(DailyCurrencyRuntime.CurrentCurrency);
        LoadCategory(m_CurrentCategory);
    }

    public override void OnClose(System.Action OnComplete = null)
    {
        DailyCurrencyRuntime.CurrencyChanged -= RefreshCurrencyText;
        base.OnClose(OnComplete);
    }

    private void RefreshCurrencyText(int currentCurrency)
    {
        if (m_CurrencyCount == null)
        {
            return;
        }

        m_CurrencyCount.text = currentCurrency.ToString();
    }

    private void SubscribeCategoryButton(BaseButton button, HeroInventoryCategory category)
    {
        if (button == null)
        {
            return;
        }

        button.Subscribe(() =>
        {
            LoadCategory(category);
        });
    }

    private void BuildCategoryLookup()
    {
        m_CategoryLookup.Clear();

        foreach (HeroInventoryCategory category in System.Enum.GetValues(typeof(HeroInventoryCategory)))
        {
            m_CategoryLookup[category] = new HeroInventoryCategoryData
            {
                Category = category,
                Items = new List<HeroInventoryItemData>()
            };
        }

        foreach (HeroInventoryCategoryData categoryData in m_CategoryData)
        {
            if (categoryData == null)
            {
                continue;
            }

            m_CategoryLookup[categoryData.Category] = categoryData;
        }
    }

    private void LoadCategory(HeroInventoryCategory category)
    {
        m_CurrentCategory = category;
        ClearItemSlots();
        RefreshCategoryTitle(category);
        RefreshCategoryButtonVisuals();

        if (!m_CategoryLookup.TryGetValue(category, out HeroInventoryCategoryData categoryData) || categoryData == null)
        {
            return;
        }

        int itemCount = Mathf.Min(m_ItemUIs.Count, categoryData.Items.Count);
        for (int i = 0; i < itemCount; i++)
        {
            HeroInventoryItemData itemData = categoryData.Items[i];
            ShopItemUI itemUI = m_ItemUIs[i];

            if (itemData == null || itemUI == null)
            {
                continue;
            }

            bool showPurchaseButton = !itemData.IsUnlocked;
            bool showEquipButton = itemData.IsUnlocked;
            itemUI.Setup(itemData.ItemIcon, showPurchaseButton, showEquipButton, () => EquipItem(itemData));
        }
    }

    private void CacheCategoryButtonVisual(BaseButton button, HeroInventoryCategory category)
    {
        if (button == null || button.Button == null)
        {
            return;
        }

        Image buttonImage = button.Button.GetComponent<Image>();
        if (buttonImage == null)
        {
            return;
        }

        m_CategoryButtonImages[category] = buttonImage;
        m_CategoryButtonInitialColors[category] = buttonImage.color;
    }

    private void RefreshCategoryButtonVisuals()
    {
        foreach (KeyValuePair<HeroInventoryCategory, Image> buttonEntry in m_CategoryButtonImages)
        {
            HeroInventoryCategory category = buttonEntry.Key;
            Image buttonImage = buttonEntry.Value;

            if (buttonImage == null || !m_CategoryButtonInitialColors.TryGetValue(category, out Color initialColor))
            {
                continue;
            }

            buttonImage.color = category == m_CurrentCategory
                ? GetSelectedCategoryColor(initialColor)
                : initialColor;
        }
    }

    private Color GetSelectedCategoryColor(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out float hue, out float saturation, out float value);

        float complementaryHue = Mathf.Repeat(hue + 0.5f, 1f);
        float selectedSaturation = Mathf.Clamp01(Mathf.Max(0.35f, saturation));
        float selectedValue = Mathf.Clamp01(Mathf.Max(0.9f, value * 1.1f));

        Color selectedColor = Color.HSVToRGB(complementaryHue, selectedSaturation, selectedValue);
        selectedColor.a = baseColor.a;
        return selectedColor;
    }

    private void RefreshCategoryTitle(HeroInventoryCategory category)
    {
        if (m_CategoryTitleText == null)
        {
            return;
        }

        m_CategoryTitleText.text = GetCategoryDisplayName(category);
    }

    private string GetCategoryDisplayName(HeroInventoryCategory category)
    {
        switch (category)
        {
            case HeroInventoryCategory.Hats:
                return "Hats";
            case HeroInventoryCategory.Accessories:
                return "Accessories";
            case HeroInventoryCategory.Tops:
                return "Tops";
            case HeroInventoryCategory.Bottoms:
                return "Bottoms";
            case HeroInventoryCategory.Shoes:
                return "Shoes";
            default:
                return category.ToString();
        }
    }

    private void ClearItemSlots()
    {
        foreach (ShopItemUI itemUI in m_ItemUIs)
        {
            if (itemUI == null)
            {
                continue;
            }

            itemUI.Clear();
        }
    }

    private void EquipItem(HeroInventoryItemData itemData)
    {
        if (itemData == null)
        {
            return;
        }

        DisableEquippedItemForCategory(m_CurrentCategory);
        m_EquippedItemsByCategory[m_CurrentCategory] = itemData;

        if (itemData.ClothingObjectToEnable != null)
        {
            itemData.ClothingObjectToEnable.SetActive(true);
        }
    }

    private void RemoveEquippedItem()
    {
        DisableEquippedItemForCategory(m_CurrentCategory);
        m_EquippedItemsByCategory.Remove(m_CurrentCategory);
    }

    private void DisableEquippedItemForCategory(HeroInventoryCategory category)
    {
        if (!m_EquippedItemsByCategory.TryGetValue(category, out HeroInventoryItemData equippedItem) || equippedItem == null)
        {
            return;
        }

        if (equippedItem.ClothingObjectToEnable != null)
        {
            equippedItem.ClothingObjectToEnable.SetActive(false);
        }
    }
}
