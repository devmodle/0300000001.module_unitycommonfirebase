using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if FIREBASE_MODULE_ENABLE
using Firebase;

#if FIREBASE_AUTH_ENABLE
using Firebase.Auth;
#endif			// #if FIREBASE_AUTH_ENABLE

#if FIREBASE_ANALYTICS_ENABLE
using Firebase.Analytics;
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if FIREBASE_CRASHLYTICS_ENABLE
using Firebase.Crashlytics;
#endif			// #if FIREBASE_CRASHLYTICS_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
using Firebase.Messaging;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE

/** 파이어 베이스 관리자 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	/** 식별자 */
	private enum EKey {
		NONE = -1,
		IS_INIT,
		MSG_TOKEN,
		[HideInInspector] MAX_VAL
	}

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
		LOAD_DATAS,
		SAVE_DATAS,
#endif			// #if FIREBASE_DB_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
		LOAD_MSG_TOKEN,
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE

		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams {
		public Dictionary<ECallback, System.Action<CFirebaseManager, bool>> m_oCallbackDict;
	}

	#region 변수
	private FirebaseApp m_oFirebaseApp = null;
	#endregion			// 변수

	#region 프로퍼티
	public STParams Params { get; private set; }

	public bool IsLogin {
		get {
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
			return this.BoolDict.GetValueOrDefault(EKey.IS_INIT) && FirebaseAuth.DefaultInstance.CurrentUser != null;
#else
			return false;
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
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

	public bool IsInit => this.BoolDict.GetValueOrDefault(EKey.IS_INIT);
	public string MsgToken => this.StrDict.GetValueOrDefault(EKey.MSG_TOKEN, string.Empty);

	/** =====> 기타 <===== */
	private Dictionary<EKey, bool> BoolDict { get; } = new Dictionary<EKey, bool>();
	private Dictionary<EKey, string> StrDict { get; } = new Dictionary<EKey, string>();
	private Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, bool>> CallbackDict01 { get; } = new Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, bool>>();
	private Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, string, bool>> CallbackDict02 { get; } = new Dictionary<EFirebaseCallback, System.Action<CFirebaseManager, string, bool>>();
	#endregion			// 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams) {
		CFunc.ShowLog($"CFirebaseManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.BoolDict.GetValueOrDefault(EKey.IS_INIT)) {
			a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.BoolDict.GetValueOrDefault(EKey.IS_INIT));
		} else {
			this.Params = a_stParams;
			CTaskManager.Inst.WaitAsyncTask(FirebaseApp.CheckAndFixDependenciesAsync(), this.OnInit);
		}
#else
		a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, false);
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}

	/** 크래시 유저 식별자를 변경한다 */
	public void SetCrashlyticsUserID(string a_oID) {
		CFunc.ShowLog($"CFirebaseManager.SetCrashlyticsUserID: {a_oID}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oID.ExIsValid());

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CRASHLYTICS_ENABLE
		// 초기화 되었을 경우
		if(this.BoolDict.GetValueOrDefault(EKey.IS_INIT)) {
			Crashlytics.SetUserId(a_oID);
		}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CRASHLYTICS_ENABLE
	}

	/** 크래시 데이터를 변경한다 */
	public void SetCrashlyticsDatas(Dictionary<string, string> a_oDataDict) {
		CFunc.ShowLog($"CFirebaseManager.SetCrashlyticsDatas: {a_oDataDict}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oDataDict.ExIsValid());

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CRASHLYTICS_ENABLE
		// 초기화 되었을 경우
		if(this.BoolDict.GetValueOrDefault(EKey.IS_INIT)) {
			foreach(var stKeyVal in a_oDataDict) {
				Crashlytics.SetCustomKey(stKeyVal.Key, stKeyVal.Value);
			}
		}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CRASHLYTICS_ENABLE
	}

	/** 메세지 토큰을 로드한다 */
	public void LoadMsgToken(System.Action<CFirebaseManager, string, bool> a_oCallback) {
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CLOUD_MSG_ENABLE
		// 초기화 되었을 경우
		if(this.BoolDict.GetValueOrDefault(EKey.IS_INIT)) {
			this.CallbackDict02.ExReplaceVal(EFirebaseCallback.LOAD_MSG_TOKEN, a_oCallback);
			CTaskManager.Inst.WaitAsyncTask(FirebaseMessaging.GetTokenAsync(), this.OnLoadMsgToken);
		} else {
			CFunc.Invoke(ref a_oCallback, this, string.Empty, false);	
		}
#else
		CFunc.Invoke(ref a_oCallback, this, string.Empty, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CLOUD_MSG_ENABLE
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	// 초기화 되었을 경우
	private void OnInit(Task<DependencyStatus> a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		this.BoolDict.ExReplaceVal(EKey.IS_INIT, a_oTask.ExIsCompleteSuccess() && a_oTask.Result == DependencyStatus.Available);
		
		CFunc.ShowLog($"CFirebaseManager.OnInit: {this.BoolDict.GetValueOrDefault(EKey.IS_INIT)}, {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);
		
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_INIT_CALLBACK, () => {
			// 초기화 되었을 경우
			if(this.BoolDict.GetValueOrDefault(EKey.IS_INIT)) {
				m_oFirebaseApp = FirebaseApp.DefaultInstance;

#if FIREBASE_ANALYTICS_ENABLE
				FirebaseAnalytics.SetSessionTimeoutDuration(KCDefine.U_TIMEOUT_FIREBASE_SESSION);
			
#if ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD
				FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#else
				FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
#endif			// #if ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
				FirebaseMessaging.TokenReceived += this.OnReceiveMsgToken;
				FirebaseMessaging.MessageReceived += this.OnReceiveNotiMsg;

				FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE
			}

			this.Params.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.BoolDict.GetValueOrDefault(EKey.IS_INIT));
		});
	}

#if FIREBASE_CLOUD_MSG_ENABLE
	/** 메세지 토큰을 로드했을 경우 */
	private void OnLoadMsgToken(Task<string> a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog($"CFirebaseManager.OnLoadMsgToken: {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_LOAD_MSG_TOKEN_CALLBACK, () => {
			this.StrDict.ExReplaceVal(EKey.MSG_TOKEN, a_oTask.ExIsCompleteSuccess() ? a_oTask.Result : string.Empty);
			this.CallbackDict02.GetValueOrDefault(EFirebaseCallback.LOAD_MSG_TOKEN)?.Invoke(this, this.StrDict.GetValueOrDefault(EKey.MSG_TOKEN, string.Empty), a_oTask.ExIsCompleteSuccess());
		});
	}

	/** 메세지 토큰을 수신했을 경우 */
	private void OnReceiveMsgToken(object a_oSender, TokenReceivedEventArgs a_oArgs) {
		CFunc.ShowLog($"CFirebaseManager.OnReceiveMsgToken: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_TOKEN_CALLBACK, () => this.StrDict.ExReplaceVal(EKey.MSG_TOKEN, a_oArgs.Token));
	}

	/** 알림 메세지를 수신했을 경우 */
	private void OnReceiveNotiMsg(object a_oSender, MessageReceivedEventArgs a_oArgs) {
		CFunc.ShowLog($"CFirebaseManager.OnReceiveNotiMsg: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);
		
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_NOTI_MSG_CALLBACK, () => {
			// Do Something
		});
	}
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
