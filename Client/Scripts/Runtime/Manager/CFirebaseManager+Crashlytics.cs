using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_CRASHLYTICS_ENABLE
using Firebase.Crashlytics;
#endif // #if FIREBASE_CRASHLYTICS_ENABLE

/** 파이어 베이스 관리자 - 크래시 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	/** 크래시 유저 식별자를 변경한다 */
	public void SetCrashlyticsUserID(string a_oID) {
		CFunc.ShowLog($"CFirebaseManager.SetCrashlyticsUserID: {a_oID}", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_oID.ExIsValid());

#if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화되었을 경우
		if(this.IsInit) {
			Crashlytics.SetUserId(a_oID);
		}
#endif // #if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	/** 크래시 데이터를 변경한다 */
	public void SetCrashlyticsDatas(Dictionary<string, string> a_oDictData) {
		CFunc.ShowLog($"CFirebaseManager.SetCrashlyticsDatas: {a_oDictData}", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_oDictData.ExIsValid());

#if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화되었을 경우
		if(this.IsInit) {
			foreach(var stKeyVal in a_oDictData) {
				Crashlytics.SetCustomKey(stKeyVal.Key, stKeyVal.Value);
			}
		}
#endif // #if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion // 함수
}
#endif // #if FIREBASE_MODULE_ENABLE
