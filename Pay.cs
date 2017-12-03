using UnityEngine;
using System.Collections;
using UnityEngine.Purchasing;
using System;

namespace Gj
{
	public class Pay : IStoreListener
	{
		private static IStoreController m_StoreController;
		private static IExtensionProvider m_StoreExtensionProvider;
		private static Pay m_instance;

		private static bool m_purchasing = false;
		private static Action<bool, Product, string> m_callback;

		private ConfigurationBuilder _builder;

		public static Pay getInstance ()
		{
			if (m_instance == null) {
				m_instance = new Pay ();
			}
			return m_instance;
		}

		private Pay ()
		{
			_builder = ConfigurationBuilder.Instance (StandardPurchasingModule.Instance ());
		}

		public static void AddConsumProduct (string productId, string storeName)
		{
			var instance = getInstance ();
			if (instance.IsInitialized ())
				return;
			getInstance ().AddProduct (productId, storeName, ProductType.Consumable);
			return;
		}

		public static void BuyProduct (string productId, Action<bool, Product, string> CB)
		{
			var instance = getInstance ();
			if (!instance.IsInitialized ()) {
				CB(false, null, "buy product need init!");
				return;
			}
			getInstance ().BuyProductID (productId, CB);
		}

		public static void InitializePurchasing ()
		{
			var instance = getInstance ();
			if (instance.IsInitialized ())
				return;
			getInstance ().Initialize ();
			return;
		}

		private void Initialize ()
		{
			UnityPurchasing.Initialize (this, _builder);
		}

		private bool IsInitialized ()
		{
			return m_StoreController != null && m_StoreExtensionProvider != null;
		}

		public void AddProduct (string productId, string storeName, ProductType type)
		{
			_builder.AddProduct (productId, type, new IDs () {
				{ productId, storeName }
			});
		}

		public void BuyProductID (string productId, Action<bool, Product, string> callback)
		{
			if (IsInitialized () == false) {
				Debug.Log ("BuyProductID FAIL. Not initialized.");
				callback (false, null, "BuyProductID FAIL. Not initialized.");
				return;
			}
			if (m_purchasing == true) {
				Debug.Log ("BuyProductID FAIL. Purchasing.");
				callback (false, null, "BuyProductID FAIL. Purchasing.");
				return;
			}
			Product product = m_StoreController.products.WithID (productId);
			if (product != null && product.availableToPurchase) {
				m_purchasing = true;
				m_callback = callback;

				Debug.Log (string.Format ("Purchasing product asychronously: '{0}'", product.definition.id));
				m_StoreController.InitiatePurchase (product);
			} else {
				Debug.Log ("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
				callback (false, null, "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
				return;
			}
		}

		public void RestorePurchases ()
		{
			if (!IsInitialized ()) {
				Debug.Log ("RestorePurchases FAIL. Not initialized.");
				return;
			}

			if (Application.platform == RuntimePlatform.IPhonePlayer ||
			    Application.platform == RuntimePlatform.OSXPlayer) {
				Debug.Log ("RestorePurchases started ...");

				var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions> ();

				apple.RestoreTransactions ((result) => {
					Debug.Log ("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
				});

			} else {
				Debug.Log ("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
			}
		}

		public void OnInitialized (IStoreController controller, IExtensionProvider extensions)
		{
			Debug.Log ("OnInitialized: PASS");
			m_StoreController = controller;
			m_StoreExtensionProvider = extensions;
		}

		public void OnInitializeFailed (InitializationFailureReason error)
		{
			Debug.Log ("OnInitializeFailed InitializationFailureReason:" + error);
		}

		public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs args)
		{
			m_purchasing = false;
			if (m_callback != null)
				m_callback (true, args.purchasedProduct, "success");
			return PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed (Product product, PurchaseFailureReason failureReason)
		{
			m_purchasing = false;
			if (m_callback != null)
				m_callback (false, null, failureReason.ToString());
			Debug.Log (string.Format ("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
		}
	}
}

