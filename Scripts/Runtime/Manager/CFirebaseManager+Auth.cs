using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_AUTH_ENABLE
using Firebase.Auth;
#endif			// #if FIREBASE_AUTH_ENABLE

//! 파이어 베이스 관리자 - 인증
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 익명 로그인을 처리한다
	public void Login(System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Login", KCDefine.B_LOG_COLOR_PLUGIN);

#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
			m_oLoginCallback = a_oCallback;
			var oTask = FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();

			CTaskManager.Instance.WaitAsyncTask(oTask, this.OnLogin);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 페이스 북 로그인을 처리한다
	public void LoginWithFacebook(string a_oAccessToken, 
		System.Action<CFirebaseManager, bool> a_oCallback) 
	{
		CFunc.ShowLog("CFirebaseManager.LoginWithFacebook: {0}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oAccessToken);
			
#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		CAccess.Assert(a_oAccessToken.ExIsValid());

		var oCredential = FacebookAuthProvider.GetCredential(a_oAccessToken);
		this.LoginWithCredential(oCredential, a_oCallback);
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 게임 센터 로그인을 처리한다
	public void LoginWithGameCenter(string a_oAuthCode, 
		System.Action<CFirebaseManager, bool> a_oCallback) 
	{
		CFunc.ShowLog("CFirebaseManager.LoginWithGameCenter: {0}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oAuthCode);

#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		m_oLoginCallback = a_oCallback;

#if UNITY_IOS
		CTaskManager.Instance.WaitAsyncTask(GameCenterAuthProvider.GetCredentialAsync(), 
			this.OnReceiveGameCenterCredential);
#else
		CAccess.Assert(a_oAuthCode.ExIsValid());

		var oCredential = PlayGamesAuthProvider.GetCredential(a_oAuthCode);
		this.LoginWithCredential(oCredential, a_oCallback);
#endif			// #if UNITY_IOS
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 로그아웃을 처리한다
	public void Logout(System.Action<CFirebaseManager> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			FirebaseAuth.DefaultInstance.SignOut();
		}
#endif			// #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)

		a_oCallback?.Invoke(this);
	}
	#endregion			// 함수

	#region 조건부 함수
#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	//! 로그인 되었을 경우
	private void OnLogin(Task<FirebaseUser> a_oTask) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FIREBASE_M_LOGIN_CALLBACK, () => {
			bool bIsComplete = a_oTask.ExIsComplete();

			string oUserID = bIsComplete ? a_oTask.Result.UserId : string.Empty;
			string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;

			CFunc.ShowLog("CFirebaseManager.OnLogin: {0}, {1}, {2}", 
				KCDefine.B_LOG_COLOR_PLUGIN, bIsComplete, oUserID, oErrorMsg);

			CFunc.Invoke(ref m_oLoginCallback, this, this.IsLogin);
		});
	}

	//! 인증 로그인을 처리한다
	private void LoginWithCredential(Credential a_oCredential, 
		System.Action<CFirebaseManager, bool> a_oCallback) 
	{
		CAccess.Assert(a_oCredential != null);
		CFunc.ShowLog("CFirebaseManager.LoginWithCredential", KCDefine.B_LOG_COLOR_PLUGIN);

		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
			m_oLoginCallback = a_oCallback;
			var oTask = FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(a_oCredential);

			CTaskManager.Instance.WaitAsyncTask(oTask, this.OnLogin);
		}
	}

#if UNITY_IOS && GAME_CENTER_MODULE_ENABLE
	//! 게임 센터 인증을 수신했을 경우
	private void OnReceiveGameCenterCredential(Task<Credential> a_oTask) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FIREBASE_M_GAME_CENTER_CALLBACK, () => {
			// 게임 센터에 인증 되었을 경우
			if(a_oTask.ExIsComplete()) {
				this.LoginWithCredential(a_oTask.Result, m_oLoginCallback);
			} else {
				CFunc.Invoke(ref m_oLoginCallback, this, false);
			}
		});
	}
#endif			// #if UNITY_IOS && GAME_CENTER_MODULE_ENABLE
#endif			// #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
