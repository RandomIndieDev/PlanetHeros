using System;
using System.Collections.Generic;
using RID;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ResourceType
{
    Experience,
}

public enum UISpace
{
    Screen, World
}

public interface ICurrencyUpdateListener
{
    List<ResourceType> CurrencyToListenToForUpdates { get; }
    void OnCurrencyUpdated(ResourceType currency, double updatedCurrency, double change);
}

public class ValueManager : MonoBehaviour, ICoreSystemManager
{
        [SerializeField] CurrencyCollector m_CurrencyCollector;
        [SerializeField] HapticManager m_HapticManager;
        [SerializeField] ResourceType m_DefaultCurrency;

        public const string KEY_PREFIX_CURRENCY_MANAGER_CURRENCY_COUNT = "KEY_PREFIX_CURRENCY_MANAGER_CURRENCY_COUNT_";

        Dictionary<ResourceType, double> m_CurrencyToCountDict;
        Dictionary<ResourceType, List<ICurrencyUpdateListener>> m_CurrencyUpdateListenersDict;

        public delegate void OnCurrencyIconClickedEvent(ResourceType currency, object data);
        public OnCurrencyIconClickedEvent OnCurrencyIconClicked;

        public CurrencyCollector GetCurrencyCollector => m_CurrencyCollector;
        
        public void Init()
        {
            m_CurrencyCollector.Init(this, m_HapticManager);
            m_CurrencyUpdateListenersDict = new Dictionary<ResourceType, List<ICurrencyUpdateListener>>();

            m_CurrencyToCountDict = new Dictionary<ResourceType, double>();
            FetchCurrencies();
        }

        public void AddCurrencyUpdateListener(ICurrencyUpdateListener listener)
        {
            AddToCurrencyUpdateListenersDict(listener);
        }

        public void RemoveCurrencyUpdateListener(ICurrencyUpdateListener listener)
        {
            RemoveFromCurrencyUpdateListenersDict(listener);
        }

        void AddToCurrencyUpdateListenersDict(ICurrencyUpdateListener listener)
        {
            foreach (var currency in listener.CurrencyToListenToForUpdates)
            {
                if (m_CurrencyUpdateListenersDict.ContainsKey(currency))
                {
                    m_CurrencyUpdateListenersDict[currency].Add(listener);
                }
                else
                {
                    m_CurrencyUpdateListenersDict.Add(currency, new List<ICurrencyUpdateListener>() { listener });
                }
            }
        }

        void RemoveFromCurrencyUpdateListenersDict(ICurrencyUpdateListener listener)
        {
            foreach (var currency in listener.CurrencyToListenToForUpdates)
            {
                if (m_CurrencyUpdateListenersDict.TryGetValue(currency, out List<ICurrencyUpdateListener> listeners))
                {
                    listeners.Remove(listener);
                }
            }
        }

        void SendListenersCurrencyUpdated(ResourceType currency, double updatedCurrency, double change)
        {
            if (m_CurrencyUpdateListenersDict.TryGetValue(currency, out List<ICurrencyUpdateListener> listeners))
            {
                foreach (var listener in listeners)
                    listener.OnCurrencyUpdated(currency,updatedCurrency, change);
            }
        }

        void FetchCurrencies()
        {
            foreach (ResourceType currency in Enum.GetValues(typeof(ResourceType)))
            {
                var currencyCountStr = PlayerPrefs.GetString(KEY_PREFIX_CURRENCY_MANAGER_CURRENCY_COUNT + currency.ToString(), "0");
                m_CurrencyToCountDict[currency] = System.Convert.ToDouble(currencyCountStr);
            }
        }

        public class TransformData
        {
            public Transform TM;
            public UISpace UISpace;
        }

        public class CollectionData
        {
            public double UICount;
            public double CurrencyWeight;
        }

        Camera m_MainCam = null;

        public void CollectCurrencyWithUIEffect(ResourceType currency, TransformData from, TransformData to, CollectionData collectionData, bool isSilent = false, Action onReached = null, Action onComplete = null)
        {
            if (m_MainCam == null)
                m_MainCam = Camera.main;

            Vector3 fromPos = from.TM.position;
            if (from.UISpace == UISpace.World)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(from.TM.position);
                fromPos = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            }

            Vector3 toPos = to.TM.position;
            if (to.UISpace == UISpace.World)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(to.TM.position);
                toPos = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            }

            m_CurrencyCollector.Collect(currency, fromPos, collectionData.UICount, collectionData.CurrencyWeight, toPos, isSilent, onReached ,onComplete);
        }

        public void SpendCurrencyWithUIEffect(ResourceType currency, TransformData from, TransformData to, double spendCount, bool isSilent = false, Action onComplete = null)
        {
            if (m_MainCam == null)
                m_MainCam = Camera.main;

            Vector3 fromPos = from.TM.position;
            if (from.UISpace == UISpace.World)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(from.TM.position);
                fromPos = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            }

            Vector3 toPos = to.TM.position;
            if (to.UISpace == UISpace.World)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(to.TM.position);
                toPos = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            }

            m_CurrencyCollector.Spend(currency, fromPos, toPos, spendCount, isSilent, onComplete);
        }
        
        public double GetCurrencyCount(ResourceType currency) => m_CurrencyToCountDict[currency];

        public void SetCurrencyCount(ResourceType currency, double value)
        {
            m_CurrencyToCountDict[currency] = value;
            PlayerPrefs.SetString(GetDBKey(currency), value.ToString());
            PlayerPrefs.Save();

            SendListenersCurrencyUpdated(currency, value, 0);
        }

        public void IncrementCurrencyCountBy(ResourceType currency, double value, bool sendEvent = true)
        {
            m_CurrencyToCountDict[currency] += value;
            
            SetCurrencyCount(currency, m_CurrencyToCountDict[currency]);
            
            SendListenersCurrencyUpdated(currency, m_CurrencyToCountDict[currency], value);
        }

        public void ReduceCurrencyCountBy(ResourceType currency, double value)
        {
            var currentValue = m_CurrencyToCountDict[currency];
            currentValue = currentValue - value;
            if (currentValue < 0)
                currentValue = 0;

            m_CurrencyToCountDict[currency] = currentValue;
            
            SetCurrencyCount(currency, m_CurrencyToCountDict[currency]);
            
            SendListenersCurrencyUpdated(currency, m_CurrencyToCountDict[currency], -value);
        }

        public void RemoveCurrency(ResourceType currency)
        {
            SetCurrencyCount(currency, 0);
        }
        
        public Sprite GetCurrencySprite(ResourceType currency)
        {
            return m_CurrencyCollector.CurrencyCollectorConfigDict[currency].CurrencySprite;
        }

        [Button]
        void DumpAllCurrencies()
        {
            foreach (ResourceType currency in Enum.GetValues(typeof(ResourceType)))
            {
                Debug.Log(currency.ToString() + " : " + m_CurrencyToCountDict[currency]);
            }
        }    
        
        string GetDBKey(ResourceType currency) => KEY_PREFIX_CURRENCY_MANAGER_CURRENCY_COUNT + currency.ToString();
}