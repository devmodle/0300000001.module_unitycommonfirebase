using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE && FIREBASE_CRASHLYTICS_ENABLE
using Firebase.Crashlytics;

//! 파이어 베이스 관리자 - 크래시 리포트
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 크래시 유저 식별자를 변경한다
	public void SetCrashUserID(string a_oID) {
		CFunc.ShowLog("CFirebaseManager.SetCrashUserID: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oID);

		// 초기화 되었을 경우
		if(this.IsInit) {
			Crashlytics.SetUserId(a_oID);
		}
	}

	//! 크래시 데이터를 변경한다
	public void SetCrashDatas(Dictionary<string, string> a_oDataList) {
		CFunc.ShowLog("CFirebaseManager.SetCrashDatas: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oDataList);

		// 초기화 되었을 경우
		if(this.IsInit) {
			foreach(var stKeyValue in a_oDataList) {
				Crashlytics.SetCustomKey(stKeyValue.Key, stKeyValue.Value);
			}
		}
	}

	//! 크래시 로그를 전송한다
	public void SendCrashLog(string a_oMsg) {
		this.SendCrashLog(new System.Exception(a_oMsg));
	}

	//! 크래시 로그를 전송한다
	public void SendCrashLog(System.Exception a_oException) {
		CFunc.ShowLog("CFirebaseManager.SendCrashLog: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oException);

		// 초기화 되었을 경우
		if(this.IsInit) {
			Crashlytics.LogException(a_oException);
		}
	}
	#endregion			// 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE && FIREBASE_CRASHLYTICS_ENABLE
