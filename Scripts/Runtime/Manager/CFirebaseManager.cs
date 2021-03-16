using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
using Firebase;

#if FIREBASE_AUTH_ENABLE
using Firebase.Auth;
#endif			// #if FIREBASE_AUTH_ENABLE

#if FIREBASE_ANALYTICS_ENABLE
using Firebase.Analytics;
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if FIREBASE_REMOTE_CONFIG_ENABLE
using Firebase.RemoteConfig;
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
using Firebase.Messaging;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE

//! 파이어 베이스 관리자
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	//! 매개 변수
	public struct STParams {
		public Dictionary<string, object> m_oConfigList;
	}

	#region 변수
	private STParams m_stParams;
	private System.Action<CFirebaseManager, bool> m_oInitCallback = null;

#if FIREBASE_AUTH_ENABLE
	private System.Action<CFirebaseManager, bool> m_oLoginCallback = null;
#endif			// #if FIREBASE_AUTH_ENABLE

#if FIREBASE_DB_ENABLE
	private System.Action<CFirebaseManager, bool> m_oSaveDBCallback = null;
	private System.Action<CFirebaseManager, string, bool> m_oLoadDBCallback = null;
#endif			// #if FIREBASE_DB_ENABLE

#if FIREBASE_REMOTE_CONFIG_ENABLE
	private bool m_bIsSetupDefConfigs = false;
	private System.Action<CFirebaseManager, bool> m_oLoadConfigCallback = null;
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit { get; private set; } = false;
	public string MsgToken { get; private set; } = string.Empty;

	public bool IsLogin {
		get {
#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
			return this.IsInit && FirebaseAuth.DefaultInstance.CurrentUser != null;
#else
			return false;
#endif			// #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		}
	}

	public bool IsSetupDefConfigs {
		get {
#if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
			return this.IsInit && m_bIsSetupDefConfigs;
#else
			return false;
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		}
	}

	public string UserID {
		get {
#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
			return this.IsLogin ? FirebaseAuth.DefaultInstance.CurrentUser.UserId : string.Empty;
#else
			return string.Empty;
#endif			// #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		}
	}
	#endregion			// 프로퍼티

	#region 함수
	//! 초기화
	public virtual void Init(STParams a_stParams, System.Action<CFirebaseManager, bool> a_oCallback) {
		CAccess.Assert(a_stParams.m_oConfigList != null);
		CFunc.ShowLog("CFirebaseManager.Init: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_stParams.m_oConfigList);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_oCallback?.Invoke(this, true);
		} else {
			m_stParams = a_stParams;
			m_oInitCallback = a_oCallback;

			CTaskManager.Inst.WaitAsyncTask(FirebaseApp.CheckAndFixDependenciesAsync(), this.OnInit);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	//! 초기화 되었을 경우
	private void OnInit(Task<DependencyStatus> a_oTask) {
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_INIT_CALLBACK, () => {
			this.IsInit = a_oTask.Result == DependencyStatus.Available;
			string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
			
			CFunc.ShowLog("CFirebaseManager.OnInit: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, this.IsInit, oErrorMsg);

			// 초기화 되었을 경우
			if(this.IsInit) {
				var oApp = FirebaseApp.DefaultInstance;

#if UNITY_EDITOR && FIREBASE_DB_ENABLE
				string oURL = CPluginInfoTable.Inst.FirebaseDBURL;
				oApp.Options.DatabaseUrl = new System.Uri(oURL);
#endif			// #if UNITY_EDITOR && FIREBASE_DB_ENABLE

#if FIREBASE_ANALYTICS_ENABLE
				FirebaseAnalytics.SetSessionTimeoutDuration(KCDefine.U_TIMEOUT_FIREBASE_SESSION);
			
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
				FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#else
				FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if FIREBASE_REMOTE_CONFIG_ENABLE
				// 속성이 유효 할 경우
				if(m_stParams.m_oConfigList != null) {
					CTaskManager.Inst.WaitAsyncTask(FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(m_stParams.m_oConfigList), this.OnSetupDefConfigs);
				}
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
				FirebaseMessaging.TokenReceived += this.OnReceiveToken;
				FirebaseMessaging.MessageReceived += this.OnReceiveMsg;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE
			}

			CFunc.Invoke(ref m_oInitCallback, this, this.IsInit);
		});
	}

#if FIREBASE_REMOTE_CONFIG_ENABLE
	//! 기본 속성을 설정했을 경우
	private void OnSetupDefConfigs(Task a_oTask) {
		m_bIsSetupDefConfigs = a_oTask.ExIsComplete();
	}
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
