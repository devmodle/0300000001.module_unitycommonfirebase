using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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

/** 파이어 베이스 관리자 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	/** 콜백 */
	public enum ECallback {
		NONE = -1,
		INIT,
		[HideInInspector] MAX_VAL
	}

	/** 파이어 베이스 콜백 */
	private enum EFirebaseCallback {
		NONE = -1,

#if FIREBASE_AUTH_ENABLE
		LOGIN,
#endif			// #if FIREBASE_AUTH_ENABLE

#if FIREBASE_DB_ENABLE
		SAVE_DB,
		LOAD_DB,
#endif			// #if FIREBASE_DB_ENABLE

#if FIREBASE_REMOTE_CONFIG_ENABLE
		LOAD_CONFIG,
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE

		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams {
		public Dictionary<string, object> m_oConfigDict;
		public Dictionary<ECallback, System.Action<CFirebaseManager, bool>> m_oCallbackDict;
	}

	#region 변수
	private STParams m_stParams;
	private bool m_bIsSetupDefConfigs = false;
	private FirebaseApp m_oFirebaseApp = null;

	private Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, bool>> m_oCallbackDict01 = new Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, bool>>();
	private Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, string, bool>> m_oCallbackDict02 = new Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, string, bool>>();
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit { get; private set; } = false;
	public string MsgToken { get; private set; } = string.Empty;

	public bool IsLogin {
		get {
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
			return this.IsInit && FirebaseAuth.DefaultInstance.CurrentUser != null;
#else
			return false;
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
		}
	}

	public bool IsSetupDefConfigs {
		get {
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
			return this.IsInit && m_bIsSetupDefConfigs;
#else
			return false;
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_REMOTE_CONFIG_ENABLE
		}
	}

	public string UserID {
		get {
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
			return this.IsLogin ? FirebaseAuth.DefaultInstance.CurrentUser.UserId : string.Empty;
#else
			return string.Empty;
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
		}
	}
	#endregion			// 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams) {
		CFunc.ShowLog($"CFirebaseManager.Init: {a_stParams.m_oConfigDict}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_stParams.m_oConfigDict != null);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, true);
		} else {
			m_stParams = a_stParams;
			CTaskManager.Inst.WaitAsyncTask(FirebaseApp.CheckAndFixDependenciesAsync(), this.OnInit);
		}
#else
		a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, false);
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	// 초기화 되었을 경우
	private void OnInit(Task<DependencyStatus> a_oTask) {
		this.IsInit = a_oTask.ExIsCompleteSuccess() && a_oTask.Result == DependencyStatus.Available;
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		
		CFunc.ShowLog($"CFirebaseManager.OnInit: {this.IsInit}, {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);
		
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_INIT_CALLBACK, () => {
			// 초기화 되었을 경우
			if(this.IsInit) {
				m_oFirebaseApp = FirebaseApp.DefaultInstance;

#if FIREBASE_ANALYTICS_ENABLE
				FirebaseAnalytics.SetSessionTimeoutDuration(KCDefine.U_TIMEOUT_FIREBASE_SESSION);
			
#if ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD
				FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#else
				FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
#endif			// #if ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if FIREBASE_REMOTE_CONFIG_ENABLE
				// 속성이 유효 할 경우
				if(m_stParams.m_oConfigDict != null) {
					CTaskManager.Inst.WaitAsyncTask(FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(m_stParams.m_oConfigDict), this.OnSetupDefConfigs);
				}
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
				FirebaseMessaging.TokenReceived += this.OnReceiveToken;
				FirebaseMessaging.MessageReceived += this.OnReceiveNotiMsg;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE
			}

			m_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.IsInit);
		});
	}

#if FIREBASE_REMOTE_CONFIG_ENABLE
	/** 기본 속성을 설정했을 경우 */
	private void OnSetupDefConfigs(Task a_oTask) {
		m_bIsSetupDefConfigs = a_oTask.ExIsCompleteSuccess();
	}
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
