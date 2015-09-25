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
        private OpenIABClient _openIabClient;
        private const string SkuPremium = PlanformDependedSettings.SkuPremium;
        
        public void Start()
        {
            var options = new Options
            {
                checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2,
                discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2,
                checkInventory = false,
                verifyMode = OptionsVerifyMode.VERIFY_SKIP,
                prefferedStoreNames = new[] { PlanformDependedSettings.StoreName },
                availableStoreNames = new[] { PlanformDependedSettings.StoreName },
                storeKeys = new Dictionary<string, string>
                {
                    { PlanformDependedSettings.StoreName, PlanformDependedSettings.StorePublicKey }
                },
                storeSearchStrategy = SearchStrategy.INSTALLER_THEN_BEST_FIT
            };

            _openIabClient = new OpenIABClient(options);
            _openIabClient.Purchased += Purchased;
            _openIabClient.Restored += Purchased;
            _openIabClient.Failed += Failed;

            _openIabClient.MapSku(PlanformDependedSettings.StoreName, new Dictionary<string, string>
            {
                { SkuPremium, SkuPremium }
            });
        }

        public void BuyPremium()
        {
            PremiumInfo.SetLocalizedText("%Connecting%");

            #if UNITY_EDITOR

            Purchased("TEST=0000");
        
            #else

            _openIabClient.PurchaseProduct(SkuPremium);

            #endif
        }

        public void Restore()
        {
            PremiumInfo.SetLocalizedText("%Connecting%");

            _openIabClient.Restore();
        }

        private void Purchased(Purchase purchase)
        {
            if (purchase.Sku == SkuPremium)
            {
                Purchased(purchase.Token);
            }
        }

        private void Purchased(string token)
        {
            GetComponent<PatternLock>().Open(TweenDirection.Right, new Task { Type = TaskType.CreateToken, Token = new ProtectedValue(token) });      
        }

        private void Failed(string error)
        {
            Debug.Log(GetType() + ": " + error);
            PremiumInfo.SetLocalizedText("%PurchaseFailed%");
        }
    }
}