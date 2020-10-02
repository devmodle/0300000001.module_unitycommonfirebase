using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE && FIREBASE_AUTH_ENABLE
#if UNITY_IOS || UNITY_ANDROID
using Firebase.Auth;
#endif			// #if UNITY_IOS || UNITY_ANDROID

//! 파이어 베이스 관리자 - 인증
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 익명 로그인을 처리한다
	public void Login(System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Login", KCDefine.B_LOG_COLOR_PLUGIN);

		// 로그인이 필요 없을 경우
		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
#if UNITY_IOS || UNITY_ANDROID
			CTaskManager.Instance.WaitAsyncTask(FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync(), (a_oTask) => {
				bool bIsComplete = a_oTask.ExIsComplete();

				string oUserID = bIsComplete ? a_oTask.Result.UserId : string.Empty;
				string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;

				CFunc.ShowLog("CFirebaseManager.OnLogin: {0}, {1}, {2}", 
					KCDefine.B_LOG_COLOR_PLUGIN, bIsComplete, oUserID, oErrorMsg);

				a_oCallback?.Invoke(this, this.IsLogin);
			});
#else
			a_oCallback?.Invoke(this, this.IsLogin);
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}
	}

	//! 로그아웃을 처리한다
	public void Logout(System.Action<CFirebaseManager> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			FirebaseAuth.DefaultInstance.SignOut();
		}
#endif			// #if UNITY_IOS || UNITY_ANDROID

		a_oCallback?.Invoke(this);
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	//! 인증 로그인을 처리한다
	private void LoginWithCredential(Credential a_oCredential, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoginWithCredential", KCDefine.B_LOG_COLOR_PLUGIN);

		// 로그인이 필요 없을 경우
		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
			CTaskManager.Instance.WaitAsyncTask(FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(a_oCredential), (a_oTask) => {
				bool bIsComplete = a_oTask.ExIsComplete();

				string oUserID = bIsComplete ? a_oTask.Result.UserId : string.Empty;
				string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;

				CFunc.ShowLog("CFirebaseManager.OnLoginWithCredential: {0}, {1}. {2}", 
					KCDefine.B_LOG_COLOR_PLUGIN, bIsComplete, oUserID, oErrorMsg);

				a_oCallback?.Invoke(this, this.IsLogin);
			});
		}
	}
	
#if FACEBOOK_MODULE_ENABLE
	//! 페이스 북 로그인을 처리한다
	public void LoginWithFacebook(string a_oAccessToken, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoginWithFacebook: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oAccessToken);

		// 로그인이 필요 없을 경우
		if(!this.IsInit || !CAccess.IsMobile()) {
			a_oCallback?.Invoke(this, false);
		} else {
			var oAuth = FirebaseAuth.DefaultInstance;
			var oCredential = FacebookAuthProvider.GetCredential(a_oAccessToken);

			this.LoginWithCredential(oCredential, a_oCallback);
		}
	}
#endif			// #if FACEBOOK_MODULE_ENABLE

#if GAME_CENTER_MODULE_ENABLE
	//! 게임 센터 로그인을 처리한다
	public void LoginWithGameCenter(string a_oAuthCode, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoginWithGameCenter: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oAuthCode);

		// 로그인이 필요 없을 경우
		if(!this.IsInit || !CAccess.IsMobile()) {
			a_oCallback?.Invoke(this, false);
		} else {
			var oAuth = FirebaseAuth.DefaultInstance;

#if UNITY_IOS
			m_oGameCenterLoginCallback = a_oCallback;

			CTaskManager.Instance.WaitAsyncTask(GameCenterAuthProvider.GetCredentialAsync(), (a_oTask) => 
				this.OnReceiveGameCenterCredential(a_oTask));
#else
			var oCredential = PlayGamesAuthProvider.GetCredential(a_oAuthCode);
			this.LoginWithCredential(oCredential, a_oCallback);
#endif			// #if UNITY_IOS
		}
	}

	//! 게임 센터 인증을 수신했을 경우
	private void OnReceiveGameCenterCredential(Task<Credential> a_oTask) {
		// 비동기 처리가 완료 되었을 경우
		if(a_oTask.ExIsComplete()) {
			this.LoginWithCredential(a_oTask.Result, m_oGameCenterLoginCallback);
		} else {
			m_oGameCenterLoginCallback?.Invoke(this, false);
		}
	}
#endif			// #if GAME_CENTER_MODULE_ENABLE
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE && FIREBASE_AUTH_ENABLE
