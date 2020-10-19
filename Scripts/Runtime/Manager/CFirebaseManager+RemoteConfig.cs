using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_REMOTE_CONFIG_ENABLE
using Firebase.RemoteConfig;
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE

//! 파이어 베이스 관리자 - 원격 속성
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 속성을 반환한다
	public string GetConfig(string a_oKey) {
		CFunc.ShowLog("CFirebaseManager.GetConfig: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oKey);

#if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		CAccess.Assert(a_oKey.ExIsValid());
		return this.IsInit ? FirebaseRemoteConfig.GetValue(a_oKey).StringValue : string.Empty;
#else
		return string.Empty;
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 속성을 로드한다
	public void LoadConfig(System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoadConfig", KCDefine.B_LOG_COLOR_PLUGIN);

#if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화가 필요 할 경우
		if(this.IsInit) {
			m_oLoadConfigCallback = a_oCallback;

			CTaskManager.Instance.WaitAsyncTask(FirebaseRemoteConfig.FetchAsync(KCDefine.U_TIMEOUT_FIREBASE_FETCH_CONFIG), 
				this.OnLoadConfig);
		} else {
			a_oCallback?.Invoke(this, false);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수

	#region 조건부 함수
#if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	//! 속성을 로드했을 경우
	private void OnLoadConfig(Task a_oTask) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FIREBASE_M_LOAD_CONFIG_CALLBACK, () => {
			string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
			CFunc.ShowLog("CFirebaseManager.OnLoadConfig: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

			// 속성이 로드 되었을 경우
			if(a_oTask.ExIsComplete()) {
				m_oLoadConfigCallback?.Invoke(this, FirebaseRemoteConfig.ActivateFetched());
			} else {
				m_oLoadConfigCallback?.Invoke(this, false);
			}
		});
	}
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
