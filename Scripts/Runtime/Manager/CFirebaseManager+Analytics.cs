using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE && FIREBASE_ANALYTICS_ENABLE
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
using Firebase.Analytics;
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)

#if PURCHASE_MODULE_ENABLE
using UnityEngine.Purchasing;
#endif			// #if PURCHASE_MODULE_ENABLE

//! 파이어 베이스 관리자 - 분석
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 분석 유저 식별자를 변경한다
	public void SetAnalyticsUserID(string a_oID) {
		CFunc.ShowLog("CFirebaseManager.SetAnalyticsUserID: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oID);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			FirebaseAnalytics.SetUserId(a_oID);
		}
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 분석 데이터를 변경한다
	public void SetAnalyticsDatas(Dictionary<string, string> a_oDataList) {
		CFunc.ShowLog("CFirebaseManager.SetAnalyticsDatas: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oDataList);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			foreach(var stKeyValue in a_oDataList) {
				FirebaseAnalytics.SetUserProperty(stKeyValue.Key, stKeyValue.Value);
			}
		}
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, string a_oParam) {
		this.SendLog(a_oName, a_oParam, null);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, string a_oParam, List<string> a_oDataList) {
		CFunc.ShowLog("CFirebaseManager.SendLog: {0}, {1}, {2}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oName, a_oParam, a_oDataList);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			var oDataList = a_oDataList ?? new List<string>();

#if MSG_PACK_ENABLE
			oDataList.ExAddValue(CCommonAppInfoStorage.Instance.AppInfo.DeviceID);

#if AUTO_LOG_PARAMS_ENABLE
			oDataList.ExAddValue(CCommonAppInfoStorage.Instance.PlatformName);
			oDataList.ExAddValue(CCommonUserInfoStorage.Instance.UserInfo.UserType.ToString());

			oDataList.ExAddValue(System.DateTime.UtcNow.ExToLongString());
			oDataList.ExAddValue(CCommonAppInfoStorage.Instance.AppInfo.UTCInstallTime.ExToLongString());
#endif			// #if AUTO_LOG_PARAMS_ENABLE
#endif			// #if MSG_PACK_ENABLE

			string oLog = oDataList.ExToString(KCDefine.B_TOKEN_CSV_STRING);
			FirebaseAnalytics.LogEvent(a_oName, a_oParam, oLog);
		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수

	#region 조건부 함수
#if PURCHASE_MODULE_ENABLE
	//! 결제 로그를 전송한다
	public void SendPurchaseLog(Product a_oProduct) {
		CAccess.Assert(a_oProduct != null);
		CFunc.ShowLog("CFirebaseManager.SendPurchaseLog: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oProduct);
		
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {

		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}
#endif			// #if PURCHASE_MODULE_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE && FIREBASE_ANALYTICS_ENABLE
