using System.Collections.Generic;
using Assets.Scripts.Common;
using Assets.Scripts.Views;
using OnePF;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameShop : Script
    {
        public UILabel PremiumInfo;
        private const string SkuPremium = "pinkeeper_premium";
        private OpenIABClient _openIabClient;

        public void Start()
        {
            var options = new Options
            {
                checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2,
                discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2,
                checkInventory = false,
                verifyMode = OptionsVerifyMode.VERIFY_SKIP,
                prefferedStoreNames = new[] { OpenIAB_Android.STORE_GOOGLE },
                storeKeys = new Dictionary<string, string>
                {
                    { PlanformDependedSettings.StoreName, PlanformDependedSettings.StorePublicKey }
                }
            };

            _openIabClient = new OpenIABClient(options);
            _openIabClient.PurchaseSucceeded += PurchaseSucceeded;
            _openIabClient.PurchaseFailed += PurchaseFailed;
        
            _openIabClient.MapSku(PlanformDependedSettings.StoreName, new Dictionary<string, string>
            {
                { SkuPremium, SkuPremium }
            });
        }

        public void BuyPremium()
        {
            PremiumInfo.SetLocalizedText("%Connecting%");

#if UNITY_EDITOR

            PurchaseSucceeded("TEST=0000");
        
#else

        _openIabClient.PurchaseProduct(SkuPremium);

        #endif
        }

        private void PurchaseSucceeded(Purchase purchase)
        {
            if (purchase.Sku == SkuPremium)
            {
                PurchaseSucceeded(purchase.Token);
            }
        }

        private void PurchaseSucceeded(string token)
        {
            GetComponent<PatternLock>().Open(TweenDirection.Right, new Task { Type = TaskType.CreateToken, Token = new ProtectedValue(token) });      
        }

        private void PurchaseFailed(string error)
        {
            Debug.Log(GetType() + ": " + error);
            PremiumInfo.SetLocalizedText("%PurchaseFailed%");
        }
    }
}