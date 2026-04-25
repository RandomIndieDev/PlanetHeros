using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using RID;
using Random = UnityEngine.Random;
using Sirenix.Utilities;
using UnityEngine.UI;

namespace RID
{
    [System.Serializable]
    public class CurrencyCollectorConfigDict : UnitySerializedDictionary<ResourceType, CurrencyCollectorConfig> { }
    [System.Serializable]
    public class CurrencyCollectorConfig
    {
        public Sprite CurrencySprite;
    }

    public class CurrencyCollector : MonoBehaviour
    {
        public CurrencyCollectorConfigDict CurrencyCollectorConfigDict;
        public List<ResourceItemUI> CurrencyItemUIs;

        public Sprite GetSpriteForCurrency(ResourceType resourceType)
        {
            return CurrencyCollectorConfigDict[resourceType].CurrencySprite;
        }

        int currentItemIndex = 0;

        ValueManager m_ValueManager;
        HapticManager m_HapticManager;

        public void Init(ValueManager currencyManager, HapticManager hapticManager)
        {
            m_ValueManager = currencyManager;
            m_HapticManager = hapticManager;
        }

        public void Collect(ResourceType currency, Transform fromTM, double count, Transform toTM, bool isSilent, Action onReached = null, Action onAnimComplete = null)
        {
            double weight = 1;
            if (count >= 10)
            {
                weight = count / 10.0F;
                count = 10;
            }
            StartCoroutine(CollectRoutine(currency, fromTM.position, toTM.position, count, weight, true, isSilent, onAnimComplete));
        }

        public void Collect(ResourceType currency, Vector3 from, double count, double weight, Vector3 toPos, bool isSilent, Action onReached = null, Action onAnimComplete = null)
        {
            StartCoroutine(CollectRoutine(currency, from, toPos, count, weight, true, isSilent, onReached, onAnimComplete));
        }
        
        public void Spend(ResourceType currency, Vector3 fromPos, Vector3 toPos, double count, bool isSilent, Action onAnimComplete = null)
        {
            double weight = 1;
            if (count >= 10)
            {
                weight = count / 10.0F;
                count = 10;
            }

            StartCoroutine(CollectRoutine(currency, fromPos, toPos, count, weight, false, isSilent, onAnimComplete));
        }

        IEnumerator CollectRoutine(ResourceType currency, Vector3 from, Vector3 to, double count, double weight, bool isCollect = true, bool isSilent = false,Action onReached = null, Action onAnimComplete = null)
        {
            int m_TrackerIndex = 0;
            var imageSprite = CurrencyCollectorConfigDict[currency].CurrencySprite;

            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSeconds(0.1F);

                var item = GetResourceItemUI();
                item.gameObject.SetActive(false);
                item.CurrencyImage.sprite = imageSprite;
                item.transform.position = from;
                item.gameObject.SetActive(true);

                float deltaY = isCollect ? -Random.Range(50, 75) : Random.Range(50, 75);
                float deltaX = Random.Range(-50, 50);
                item.transform.DOMove(from + new Vector3(deltaX, deltaY, 0), 0.2F).OnComplete(() =>
                {
                    item.transform.DOMove(to, 0.5F).SetEase(Ease.InSine).OnComplete(() =>
                    {
                        if (!isSilent)
                        {
                            if (isCollect)
                                m_ValueManager.IncrementCurrencyCountBy(currency, weight);
                            else
                                m_ValueManager.ReduceCurrencyCountBy(currency, weight);
                        }

                        item.gameObject.SetActive(false);
                        
                        onReached?.Invoke();

                        if (m_TrackerIndex == count - 1)
                        {
                            onAnimComplete?.Invoke();
                        }
                        
                        m_TrackerIndex++;
                    });
                });
            }
        }

        ResourceItemUI GetResourceItemUI()
        {
            int index = currentItemIndex % CurrencyItemUIs.Count;
            currentItemIndex++;
            if (currentItemIndex >= CurrencyItemUIs.Count)
            {
                currentItemIndex = 0;
            }
            return CurrencyItemUIs[index];
        }
    }
}