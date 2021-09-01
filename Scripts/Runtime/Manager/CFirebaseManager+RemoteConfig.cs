using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_REMOTE_CONFIG_ENABLE
using Firebase.RemoteConfig;
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE

//! 파이어 베이스 관리자 - 원격 속성
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 속성을 반환한다
	public string GetConfig(string a_oKey) {
		CFunc.ShowLog($"CFirebaseManager.GetConfig: {a_oKey}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oKey.ExIsValid());

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
		return this.IsSetupDefConfigs ? FirebaseRemoteConfig.DefaultInstance.GetValue(a_oKey).StringValue : string.Empty;
#else
		return string.Empty;
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
	}

	//! 속성을 로드한다
	public void LoadConfig(System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoadConfig", KCDefine.B_LOG_COLOR_PLUGIN);

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
		// 기본 속성이 설정 되었을 경우
		if(this.IsSetupDefConfigs) {
			m_oLoadConfigCallback = a_oCallback;
			CTaskManager.Inst.WaitAsyncTask(FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync(), this.OnLoadConfig);
		} else {
			a_oCallback?.Invoke(this, false);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
	}
	#endregion			// 함수

	#region 조건부 함수
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
	//! 속성이 로드 되었을 경우
	private void OnLoadConfig(Task<bool> a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog($"CFirebaseManager.OnLoadConfig: {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_LOAD_CONFIG_CALLBACK, () => {
			CFunc.Invoke(ref m_oLoadConfigCallback, this, a_oTask.ExIsComplete() && a_oTask.Result);
		});
	}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
