using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_ANALYTICS_ENABLE
using Firebase.Analytics;
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if PURCHASE_MODULE_ENABLE
using UnityEngine.Purchasing;
#endif			// #if PURCHASE_MODULE_ENABLE

//! 파이어 베이스 관리자 - 분석
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 분석 유저 식별자를 변경한다
	public void SetAnalyticsUserID(string a_oID) {
		CFunc.ShowLog("CFirebaseManager.SetAnalyticsUserID: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oID);

#if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			FirebaseAnalytics.SetUserId(a_oID);
		}
#endif			// #if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 분석 데이터를 변경한다
	public void SetAnalyticsDatas(Dictionary<string, string> a_oDataList) {
		CFunc.ShowLog("CFirebaseManager.SetAnalyticsDatas: {0}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oDataList);

#if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			foreach(var stKeyValue in a_oDataList) {
				FirebaseAnalytics.SetUserProperty(stKeyValue.Key, stKeyValue.Value);
			}
		}
#endif			// #if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName) {
		this.SendLog(a_oName, null);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, string a_oParam, Dictionary<string, string> a_oDataList) {
		this.SendLog(a_oName, new Dictionary<string, string>() {
			[a_oParam] = a_oDataList.ExToString(KCDefine.B_TOKEN_CSV_STRING)
		});
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, Dictionary<string, string> a_oDataList) {
		CFunc.ShowLog("CFirebaseManager.SendLog: {0}, {1}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oName, a_oDataList);

#if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			var oDataList = a_oDataList ?? new Dictionary<string, string>();

			oDataList.ExAddValue(KCDefine.U_LOG_KEY_DEVICE_ID, 
				CCommonAppInfoStorage.Instance.AppInfo.DeviceID);

			oDataList.ExAddValue(KCDefine.U_LOG_KEY_PLATFORM, 
				CCommonAppInfoStorage.Instance.Platform);

#if AUTO_LOG_PARAMS_ENABLE
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_USER_TYPE, 
				CCommonUserInfoStorage.Instance.UserInfo.UserType.ToString());

			oDataList.ExAddValue(KCDefine.U_LOG_KEY_LOG_TIME, 
				System.DateTime.UtcNow.ExToLongString());

			oDataList.ExAddValue(KCDefine.U_LOG_KEY_INSTALL_TIME, 
				CCommonAppInfoStorage.Instance.AppInfo.UTCInstallTime.ExToLongString());
#endif			// #if AUTO_LOG_PARAMS_ENABLE

			var oParams = this.MakeParams(oDataList);
			FirebaseAnalytics.LogEvent(a_oName, oParams);
		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수

	#region 조건부 함수
#if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	//! 매개 변수를 생성한다
	private Parameter[] MakeParams(Dictionary<string, string> a_oDataList) {
		var oParamList = new List<Parameter>();

		foreach(var stKeyValue in a_oDataList) {
			var oParam = new Parameter(stKeyValue.Key, stKeyValue.Value);
			oParamList.Add(oParam);
		}

		return oParamList.ToArray();
	}
#endif			// #if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)

#if PURCHASE_MODULE_ENABLE
	//! 결제 로그를 전송한다
	public void SendPurchaseLog(Product a_oProduct) {
		CFunc.ShowLog("CFirebaseManager.SendPurchaseLog: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oProduct);

#if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		CAccess.Assert(a_oProduct != null);
		
		// 초기화 되었을 경우
		if(this.IsInit) {
			var oParams = this.MakeParams(new Dictionary<string, string>() {
				[FirebaseAnalytics.ParameterItemId] = a_oProduct.definition.id,
				[FirebaseAnalytics.ParameterItemName] = a_oProduct.metadata.localizedTitle,
				[FirebaseAnalytics.ParameterCurrency] = a_oProduct.metadata.isoCurrencyCode,
				[FirebaseAnalytics.ParameterPrice] = a_oProduct.metadata.localizedPrice.ToString(),
				[FirebaseAnalytics.ParameterTransactionId] = a_oProduct.transactionID
			});
			
			FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, oParams);
		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if FIREBASE_ANALYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}
#endif			// #if PURCHASE_MODULE_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
