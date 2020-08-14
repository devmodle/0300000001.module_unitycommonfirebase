using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_ENABLE
using Firebase;
using Firebase.Unity.Editor;

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
	#region 변수
#if FIREBASE_AUTH_ENABLE && GAME_CENTER_ENABLE
	private System.Action<CFirebaseManager, bool> m_oGameCenterLoginCallback = null;
#endif			// #if FIREBASE_AUTH_ENABLE && GAME_CENTER_ENABLE
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit { get; private set; } = false;

#if FIREBASE_AUTH_ENABLE
	public bool IsLogin => this.IsInit && FirebaseAuth.DefaultInstance.CurrentUser != null;
	public string UserID => this.IsLogin ? FirebaseAuth.DefaultInstance.CurrentUser.UserId : string.Empty;
#endif			// #if FIREBASE_AUTH_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
	public string MsgToken { get; private set; } = string.Empty;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE
	#endregion			// 프로퍼티

	#region 함수
	//! 초기화
	public virtual void Init(Dictionary<string, object> a_oConfigDataList, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Init: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oConfigDataList);

		if(this.IsInit || !CAccess.IsMobilePlatform()) {
			a_oCallback?.Invoke(this, this.IsInit);
		} else {
			CTaskManager.Instance.WaitAsyncTask(FirebaseApp.CheckAndFixDependenciesAsync(), (a_oTask) => {
				this.IsInit = a_oTask.Result == DependencyStatus.Available;
				CFunc.ShowLog("CFirebaseManager.OnInit: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, this.IsInit, a_oTask.Exception?.Message);

				if(this.IsInit) {
#if UNITY_EDITOR && FIREBASE_DATABASE_ENABLE
					string oFirebaseURL = CPluginInfoTable.Instance.FirebasePluginInfo.m_oDatabaseURL;
					FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(oFirebaseURL);
#endif			// #if UNITY_EDITOR && FIREBASE_DATABASE_ENABLE

#if FIREBASE_ANALYTICS_ENABLE
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
					FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#else
					FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if FIREBASE_ANALYTICS_ENABLE

#if FIREBASE_REMOTE_CONFIG_ENABLE
					if(a_oConfigDataList != null) {
						FirebaseRemoteConfig.SetDefaults(a_oConfigDataList);
					}

					this.LoadConfigData(null);
#endif			// #if FIREBASE_REMOTE_CONFIG_ENABLE

#if FIREBASE_CLOUD_MSG_ENABLE
					FirebaseMessaging.TokenReceived += this.OnReceiveToken;
					FirebaseMessaging.MessageReceived += this.OnReceiveMsg;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE
				}

				a_oCallback?.Invoke(this, this.IsInit);
			});
		}
	}
	#endregion			// 함수
}
#endif			// #if FIREBASE_ENABLE
