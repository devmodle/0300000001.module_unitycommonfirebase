using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_ANALYTICS_ENABLE
using Firebase.Analytics;
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if PURCHASE_MODULE_ENABLE
using UnityEngine.Purchasing;
#endif			// #if PURCHASE_MODULE_ENABLE

/** 파이어 베이스 관리자 - 분석 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	/** 분석 유저 식별자를 변경한다 */
	public void SetAnalyticsUserID(string a_oID) {
		CFunc.ShowLog($"CFirebaseManager.SetAnalyticsUserID: {a_oID}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oID.ExIsValid());

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE
		// 초기화 되었을 경우
		if(this.IsInit) {
			FirebaseAnalytics.SetUserId(a_oID);
		}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE
	}
	
	/** 로그를 전송한다 */
	public void SendLog(string a_oName, Dictionary<string, string> a_oDataDict) {
		CFunc.ShowLog($"CFirebaseManager.SendLog: {a_oName}, {a_oDataDict}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oName.ExIsValid());

#if ((UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			var oDataDict = a_oDataDict ?? new Dictionary<string, string>();

			oDataDict.TryAdd(KCDefine.L_LOG_KEY_DEVICE_ID, CCommonAppInfoStorage.Inst.AppInfo.DeviceID);
			oDataDict.TryAdd(KCDefine.L_LOG_KEY_PLATFORM, CCommonAppInfoStorage.Inst.Platform);

#if AUTO_LOG_PARAMS_ENABLE
#if ANALYTICS_TEST_ENABLE || (DEBUG || DEVELOPMENT_BUILD)
			oDataDict.TryAdd(KCDefine.L_LOG_KEY_USER_TYPE, KCDefine.B_TEXT_UNKNOWN);
#else
			oDataDict.TryAdd(KCDefine.L_LOG_KEY_USER_TYPE, CCommonUserInfoStorage.Inst.UserInfo.UserType.ToString());
#endif			// #if ANALYTICS_TEST_ENABLE || (DEBUG || DEVELOPMENT_BUILD)

			oDataDict.TryAdd(KCDefine.L_LOG_KEY_LOG_TIME, System.DateTime.UtcNow.ExToLongStr());
			oDataDict.TryAdd(KCDefine.L_LOG_KEY_INSTALL_TIME, CCommonAppInfoStorage.Inst.AppInfo.UTCInstallTime.ExToLongStr());
#endif			// #if AUTO_LOG_PARAMS_ENABLE

			FirebaseAnalytics.LogEvent(a_oName, this.MakeParams(oDataDict).ToArray());
		}
#endif			// #if ((UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
	}
	#endregion			// 함수

	#region 조건부 함수
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE
	/** 매개 변수를 생성한다 */
	private List<Parameter> MakeParams(Dictionary<string, string> a_oDataDict) {
		CAccess.Assert(a_oDataDict != null);
		var oParamsList = new List<Parameter>();

		foreach(var stKeyVal in a_oDataDict) {
			oParamsList.ExAddVal(new Parameter(stKeyVal.Key, stKeyVal.Value));
		}

		return oParamsList
	}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE

#if PURCHASE_MODULE_ENABLE
	/** 결제 로그를 전송한다 */
	public void SendPurchaseLog(Product a_oProduct, int a_nNumProducts) {
		CFunc.ShowLog($"CFirebaseManager.SendPurchaseLog: {a_oProduct}, {a_nNumProducts}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oProduct != null);

#if ((UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			var oParamsList = this.MakeParams(new Dictionary<string, string>() {
				[FirebaseAnalytics.ParameterItemId] = a_oProduct.definition.id, [FirebaseAnalytics.ParameterItemName] = a_oProduct.metadata.localizedTitle, [FirebaseAnalytics.ParameterCurrency] = a_oProduct.metadata.isoCurrencyCode, [FirebaseAnalytics.ParameterQuantity] = $"{a_nNumProducts}", [FirebaseAnalytics.ParameterPrice] = $"{a_oProduct.metadata.localizedPrice}", [FirebaseAnalytics.ParameterTransactionId] = a_oProduct.transactionID
			});
			
			FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, oParamsList.ToArray());
		}
#endif			// #if ((UNITY_IOS || UNITY_ANDROID) && FIREBASE_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
	}
#endif			// #if PURCHASE_MODULE_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
