using System;
using System.Collections.Generic;
using System.Linq;
using OnePF;

namespace Assets.Scripts.Common
{
    internal enum StoreAction
    {
        Buy,
        Restore
    }

    public class OpenIABClient : IDisposable
    {
        public static bool EnableLog = true;
        public event Action<Purchase> PurchaseSucceeded = purchase => { }; 
        public event Action<string> PurchaseFailed = error => { };
        public event Action<Purchase> ConsumePurchaseSucceeded = purchase => { };
        public event Action<string> ConsumePurchaseFailed = error => { };
        public event Action<string> RestoreFailed = error => { };
        public event Action RestoreSucceeded = () => { };

        private bool _initialized;
        private bool _busy;
        private readonly Options _options;
        private string _sku;
        private StoreAction _action;

        public OpenIABClient(Options options)
        {
            WriteLog("initializing client...");
            WriteLog("options.checkInventoryTimeoutMs: {0}", options.checkInventoryTimeoutMs);
            WriteLog("options.discoveryTimeoutMs: {0}", options.discoveryTimeoutMs);
            WriteLog("options.verifyMode: {0}", options.verifyMode);
            WriteLog("options.prefferedStoreNames: {0}", string.Join(", ", options.prefferedStoreNames));
            WriteLog("options.storeKeys: {0}", string.Join(", ", options.storeKeys.Select(i => string.Format("{0}:{1}", i.Key, i.Value)).ToArray()));

            _options = options;

            WriteLog("subscribing events...");

            if (UnityEngine.Object.FindObjectOfType<OpenIABEventManager>() == null)
            {
                throw new Exception("OpenIABEventManager component missed");
            }

            OpenIABEventManager.billingSupportedEvent += BillingSupportedEvent;
            OpenIABEventManager.billingNotSupportedEvent += BillingNotSupportedEvent;
            OpenIABEventManager.queryInventorySucceededEvent += QueryInventorySucceededEvent;
            OpenIABEventManager.queryInventoryFailedEvent += QueryInventoryFailedEvent;
            OpenIABEventManager.purchaseSucceededEvent += PurchaseSucceededEvent;
            OpenIABEventManager.purchaseFailedEvent += PurchaseFailedEvent;
            OpenIABEventManager.consumePurchaseSucceededEvent += ConsumePurchaseSucceededEvent;
            OpenIABEventManager.consumePurchaseFailedEvent += ConsumePurchaseFailedEvent;
        }

        public void MapSku(string storeName, Dictionary<string, string> map)
        {
            foreach (var sku in map)
            {
                WriteLog("mapping sku: {0}", sku);
                OpenIAB.mapSku(sku.Key, storeName, sku.Value);
            }
        }

        public void PurchaseProduct(string sku)
        {
            WriteLog("starting purchase: {0}", sku);

            if (_busy)
            {
                ActionCompleted(PurchaseFailed, "busy");

                return;
            }

            _sku = sku;
            _action = StoreAction.Buy;
            _busy = true;

            if (_initialized)
            {
                WriteLog("quering inventory...");
                OpenIAB.queryInventory();
            }
            else
            {
                WriteLog("initializing OpenIAB...");
                OpenIAB.init(_options);
            }
        }

        public void ConsumeProduct(Purchase purchase)
        {
            WriteLog("starting consume: {0}", purchase.Sku);

            if (_busy)
            {
                ActionCompleted(ConsumePurchaseFailed, "busy");

                return;
            }

            if (!_initialized)
            {
                ActionCompleted(ConsumePurchaseFailed, "not initialized");

                return;
            }

            _busy = true;
            
            OpenIAB.consumeProduct(purchase);
        }

        public void Restore()
        {
            WriteLog("starting restore...");

            if (_busy)
            {
                ActionCompleted(RestoreFailed, "busy");

                return;
            }

            _action = StoreAction.Restore;
            _busy = true;

            if (_initialized)
            {
                OpenIAB.queryInventory();
            }
            else
            {
                OpenIAB.init(_options);
            }
        }

        public void Dispose()
        {
            WriteLog("unbinding...");
            OpenIAB.unbindService();
        }

        private void BillingSupportedEvent()
        {
            _initialized = true;

            WriteLog("billing supported, quering inventory...");
            OpenIAB.queryInventory();
        }

        private void BillingNotSupportedEvent(string error)
        {
            WriteLog("billing not supported");
            ActionCompleted(PurchaseFailed, error);
        }

        private void QueryInventorySucceededEvent(Inventory inventory)
        {
            var purchases = inventory.GetAllPurchases();

            WriteLog("query inventory succeeded, purchases count: {0}", purchases.Count);

            if (purchases.Any())
            {
                WriteLog("purchases: {0}", string.Join(", ", purchases.Select(i => i.Sku).ToArray()));
            }

            switch (_action)
            {
                case StoreAction.Buy:
                {
                    if (inventory.HasPurchase(_sku))
                    {
                        ActionCompleted(PurchaseSucceeded, inventory.GetPurchase(_sku));
                    }
                    else
                    {
                        WriteLog("purchasing product: {0}", _sku);
                        OpenIAB.purchaseProduct(_sku);
                    }

                    break;
                }
                case StoreAction.Restore:
                {
                    WriteLog("restoring purchases...");

                    foreach (var purchase in purchases)
                    {
                        WriteLog("restoring: {0}", purchase.Sku);
                        ActionCompleted(PurchaseSucceeded, purchase);
                    }

                    RestoreSucceeded();

                    break;
                }
            }
        }

        private void QueryInventoryFailedEvent(string error)
        {
            WriteLog("query inventory failed: {0}", error);
            ActionCompleted(PurchaseFailed, error);
        }

        private void PurchaseSucceededEvent(Purchase purchase)
        {
            WriteLog("purchase succeeded, json: {0})", purchase.OriginalJson);
            ActionCompleted(PurchaseSucceeded, purchase);
        }

        private void PurchaseFailedEvent(int error, string message)
        {
            WriteLog("purchase failed: {0}", message);
            ActionCompleted(PurchaseFailed, message);
        }

        private void ConsumePurchaseSucceededEvent(Purchase purchase)
        {
            WriteLog("consume succeeded, json: {0}", purchase.Sku);
            ActionCompleted(ConsumePurchaseSucceeded, purchase);
        }

        private void ConsumePurchaseFailedEvent(string error)
        {
            WriteLog("purchase failed: {0}", error);
            ActionCompleted(ConsumePurchaseFailed, error);
        }

        private void ActionCompleted(Action<string> action, string param)
        {
            WriteLog("action completed, params: {0}", param);
            _busy = false;
            action(param);
        }

        private void ActionCompleted(Action<Purchase> action, Purchase purchase)
        {
            WriteLog("action completed, purchase json: {0}", purchase.OriginalJson);
            _busy = false;
            action(purchase);
        }

        private void WriteLog(string message, params object[] args)
        {
            if (EnableLog)
            {
                UnityEngine.Debug.Log(GetType().Name + ": " + string.Format(message, args));
            }
        }
    }
}