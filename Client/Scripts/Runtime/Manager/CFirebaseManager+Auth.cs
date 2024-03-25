using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if FIREBASE_MODULE_ENABLE
using System.Threading.Tasks;

#if FIREBASE_AUTH_ENABLE
using Firebase.Auth;
#endif // #if FIREBASE_AUTH_ENABLE

/** 파이어 베이스 관리자 - 인증 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	/** 익명 로그인을 처리한다 */
	public void Login(string a_oDeviceID, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.Login: {a_oDeviceID}", KCDefine.B_LOG_COLOR_PLUGIN);

#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 로그인되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDictA.ExReplaceVal(EFirebaseCallback.LOGIN, a_oCallback);
			CTaskManager.Inst.WaitAsyncTask(FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync(), this.OnLogin);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif // #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	/** 애플 로그인을 처리한다 */
	public void LoginWithApple(string a_oUserID, string a_oIDToken, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.LoginWithApple: {a_oUserID}, {a_oIDToken}", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_oUserID.ExIsValid() && a_oIDToken.ExIsValid());

#if FIREBASE_AUTH_ENABLE && ENABLE_LOGIN_APPLE && UNITY_IOS
		var oAuth = FirebaseAuth.DefaultInstance;
		var oCredential = OAuthProvider.GetCredential(KCDefine.B_PROVIDER_ID_FIREBASE_M_APPLE_LOGIN, a_oUserID, a_oIDToken, null);

		this.LoginWithCredential(oCredential, a_oCallback);
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif // #if FIREBASE_AUTH_ENABLE && ENABLE_LOGIN_APPLE && UNITY_IOS
	}

	/** 페이스 북 로그인을 처리한다 */
	public void LoginWithFacebook(string a_oAccessToken, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.LoginWithFacebook: {a_oAccessToken}", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_oAccessToken.ExIsValid());

#if FIREBASE_AUTH_ENABLE && FACEBOOK_MODULE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		var oAuth = FirebaseAuth.DefaultInstance;
		this.LoginWithCredential(FacebookAuthProvider.GetCredential(a_oAccessToken), a_oCallback);
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif // #if FIREBASE_AUTH_ENABLE && FACEBOOK_MODULE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	/** 로그아웃을 처리한다 */
	public void Logout(System.Action<CFirebaseManager> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

		try {
#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
			// 로그인되었을 경우
			if(this.IsInit && this.IsLogin) {
				FirebaseAuth.DefaultInstance.SignOut();
			}
#endif // #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		} finally {
			CScheduleManager.Inst.AddCallback(KCDefine.B_KEY_FIREBASE_M_LOGOUT_CALLBACK, () => {
				CFunc.Invoke(ref a_oCallback, this);
			});
		}
	}

#if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	/** 로그인되었을 경우 */
	private void OnLogin(Task<FirebaseUser> a_oTask) {
		string oUserID = a_oTask.ExIsCompleteSuccess() ? a_oTask.Result.UserId : string.Empty;
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;

		CFunc.ShowLog($"CFirebaseManager.OnLogin: {a_oTask.ExIsCompleteSuccess()}, {oUserID}, {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.B_KEY_FIREBASE_M_LOGIN_CALLBACK, () => {
			m_oCallbackDictA.GetValueOrDefault(EFirebaseCallback.LOGIN)?.Invoke(this, this.IsLogin);
		});
	}

	/** 인증 로그인을 처리한다 */
	private void LoginWithCredential(Credential a_oCredential, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoginWithCredential", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_oCredential != null);

		// 로그인되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDictA.ExReplaceVal(EFirebaseCallback.LOGIN, a_oCallback);
			CTaskManager.Inst.WaitAsyncTask(FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(a_oCredential), this.OnLogin);
		}
	}
#endif // #if FIREBASE_AUTH_ENABLE && (UNITY_IOS || UNITY_ANDROID)
#endregion // 함수
}
#endif // #if FIREBASE_MODULE_ENABLE
