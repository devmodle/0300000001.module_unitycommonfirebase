using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_AUTH_ENABLE
using Firebase.Auth;
#endif			// #if FIREBASE_AUTH_ENABLE

/** 파이어 베이스 관리자 - 인증 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	/** 익명 로그인을 처리한다 */
	public void Login(System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Login", KCDefine.B_LOG_COLOR_PLUGIN);

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EFirebaseCallback.LOGIN, a_oCallback);
			CTaskManager.Inst.WaitAsyncTask(FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync(), this.OnLogin);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
	}

	/** 애플 로그인을 처리한다 */
	public void LoginWithApple(string a_oUserID, string a_oIDToken, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.LoginWithApple: {a_oUserID}, {a_oIDToken}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oUserID.ExIsValid() && a_oIDToken.ExIsValid());

#if UNITY_IOS && (FIREBASE_AUTH_ENABLE && APPLE_LOGIN_ENABLE)
		var oAuth = FirebaseAuth.DefaultInstance;
		this.LoginWithCredential(OAuthProvider.GetCredential(KCDefine.U_PROVIDER_ID_FIREBASE_M_APPLE_LOGIN, a_oUserID, a_oIDToken, null), a_oCallback);
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if UNITY_IOS && (FIREBASE_AUTH_ENABLE && APPLE_LOGIN_ENABLE)
	}

	/** 페이스 북 로그인을 처리한다 */
	public void LoginWithFacebook(string a_oAccessToken, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.LoginWithFacebook: {a_oAccessToken}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oAccessToken.ExIsValid());
			
#if (UNITY_IOS || UNITY_ANDROID) && (FIREBASE_AUTH_ENABLE && FACEBOOK_MODULE_ENABLE)
		var oAuth = FirebaseAuth.DefaultInstance;
		this.LoginWithCredential(FacebookAuthProvider.GetCredential(a_oAccessToken), a_oCallback);
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && (FIREBASE_AUTH_ENABLE && FACEBOOK_MODULE_ENABLE)
	}

	/** 게임 센터 로그인을 처리한다 */
	public void LoginWithGameCenter(string a_oAccessToken, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.LoginWithGameCenter: {a_oAccessToken}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oAccessToken.ExIsValid());

		var oAuth = FirebaseAuth.DefaultInstance;

#if (UNITY_IOS || UNITY_ANDROID) && (FIREBASE_AUTH_ENABLE && GAME_CENTER_MODULE_ENABLE)
#if UNITY_IOS
		m_oCallbackDict01.ExReplaceVal(EFirebaseCallback.LOGIN, a_oCallback);
		CTaskManager.Inst.WaitAsyncTask(GameCenterAuthProvider.GetCredentialAsync(), this.OnReceiveGameCenterCredential);	
#else
		this.LoginWithCredential(PlayGamesAuthProvider.GetCredential(a_oAccessToken), a_oCallback);
#endif			// #if UNITY_IOS
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && (FIREBASE_AUTH_ENABLE && GAME_CENTER_MODULE_ENABLE)
	}
	
	/** 로그아웃을 처리한다 */
	public void Logout(System.Action<CFirebaseManager> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			FirebaseAuth.DefaultInstance.SignOut();
		}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE

		CFunc.Invoke(ref a_oCallback, this);
	}
	#endregion			// 함수

	#region 조건부 함수
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
	/** 로그인 되었을 경우 */
	private void OnLogin(Task<FirebaseUser> a_oTask) {
		string oUserID = a_oTask.ExIsCompleteSuccess() ? a_oTask.Result.UserId : string.Empty;
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;

		CFunc.ShowLog($"CFirebaseManager.OnLogin: {a_oTask.ExIsCompleteSuccess()}, {oUserID}, {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_LOGIN_CALLBACK, () => m_oCallbackDict01.GetValueOrDefault(EFirebaseCallback.LOGIN)?.Invoke(this, this.IsLogin));
	}

	/** 인증 로그인을 처리한다 */
	private void LoginWithCredential(Credential a_oCredential, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoginWithCredential", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oCredential != null);

		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EFirebaseCallback.LOGIN, a_oCallback);
			CTaskManager.Inst.WaitAsyncTask(FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(a_oCredential), this.OnLogin);
		}
	}

#if UNITY_IOS && GAME_CENTER_MODULE_ENABLE
	/** 게임 센터 인증을 수신했을 경우 */
	private void OnReceiveGameCenterCredential(Task<Credential> a_oTask) {
		CFunc.ShowLog($"CFirebaseManager.OnReceiveGameCenterCredential: {a_oTask.ExIsCompleteSuccess()}");
		
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_RECEIVE_GAME_CENTER_CREDENTIAL_CALLBACK, () => {
			// 수신했을 경우
			if(a_oTask.ExIsCompleteSuccess()) {
				this.LoginWithCredential(a_oTask.Result, m_oCallbackDict01.GetValueOrDefault(EFirebaseCallback.LOGIN));
			} else {
				m_oCallbackDict01.GetValueOrDefault(EFirebaseCallback.LOGIN)?.Invoke(this, false);
			}
		});
	}
#endif			// #if UNITY_IOS && GAME_CENTER_MODULE_ENABLE
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_AUTH_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
