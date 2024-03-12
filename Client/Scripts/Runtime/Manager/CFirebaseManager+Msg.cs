using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if FIREBASE_MODULE_ENABLE
using System.Threading.Tasks;

#if FIREBASE_MSG_ENABLE
using Firebase.Messaging;
#endif // #if FIREBASE_MSG_ENABLE

/** 파이어 베이스 관리자 - 메세지 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	/** 메세지 토큰을 로드한다 */
	public void LoadMsgToken(System.Action<CFirebaseManager, string, bool> a_oCallback) {
#if FIREBASE_MSG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화되었을 경우
		if(this.IsInit) {
			m_oCallbackDictB.ExReplaceVal(EFirebaseCallback.LOAD_MSG_TOKEN, a_oCallback);
			CTaskManager.Inst.WaitAsyncTask(FirebaseMessaging.GetTokenAsync(), this.OnLoadMsgToken);
		} else {
			CFunc.Invoke(ref a_oCallback, this, string.Empty, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, string.Empty, false);
#endif // #if FIREBASE_MSG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

#if FIREBASE_MSG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	/** 메세지 토큰을 로드했을 경우 */
	private void OnLoadMsgToken(Task<string> a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog($"CFirebaseManager.OnLoadMsgToken: {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.B_KEY_FIREBASE_M_LOAD_MSG_TOKEN_CALLBACK, () => {
			this.MsgToken = a_oTask.ExIsCompleteSuccess() ? a_oTask.Result : string.Empty;
			
			m_oCallbackDictB.GetValueOrDefault(EFirebaseCallback.LOAD_MSG_TOKEN)?.Invoke(this, 
				this.MsgToken, a_oTask.ExIsCompleteSuccess());
		});
	}

	/** 메세지 토큰을 수신했을 경우 */
	private void OnReceiveMsgToken(object a_oSender, TokenReceivedEventArgs a_oArgs) {
		CFunc.ShowLog($"CFirebaseManager.OnReceiveMsgToken: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.B_KEY_FIREBASE_M_TOKEN_CALLBACK, () => {
			this.MsgToken = a_oArgs.Token;
		});
	}

	/** 알림 메세지를 수신했을 경우 */
	private void OnReceiveNotiMsg(object a_oSender, MessageReceivedEventArgs a_oArgs) {
		CFunc.ShowLog($"CFirebaseManager.OnReceiveNotiMsg: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.B_KEY_FIREBASE_M_NOTI_MSG_CALLBACK, () => {
			// Do Something
		});
	}
#endif // #if FIREBASE_MSG_ENABLE && (UNITY_IOS || UNITY_ANDROID)
#endregion // 함수
}
#endif // #if FIREBASE_MODULE_ENABLE
