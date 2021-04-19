using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_CRASHLYTICS_ENABLE
using Firebase.Crashlytics;
#endif			// #if FIREBASE_CRASHLYTICS_ENABLE

//! 파이어 베이스 관리자 - 크래시 리포트
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 크래시 유저 식별자를 변경한다
	public void SetCrashUserID(string a_oID) {
		CAccess.Assert(a_oID.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.SetCrashUserID: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oID);

#if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			Crashlytics.SetUserId(a_oID);
		}
#endif			// #if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 크래시 데이터를 변경한다
	public void SetCrashDatas(Dictionary<string, string> a_oDataList) {
		CFunc.ShowLog("CFirebaseManager.SetCrashDatas: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oDataList);

#if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			foreach(var stKeyVal in a_oDataList) {
				Crashlytics.SetCustomKey(stKeyVal.Key, stKeyVal.Value);
			}
		}
#endif			// #if FIREBASE_CRASHLYTICS_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
