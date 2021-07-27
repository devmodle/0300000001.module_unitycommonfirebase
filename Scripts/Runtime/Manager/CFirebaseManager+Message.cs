using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if FIREBASE_MODULE_ENABLE 
#if FIREBASE_CLOUD_MSG_ENABLE
using Firebase.Messaging;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE

//! 파이어 베이스 관리자 - 메세지
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 조건부 함수
#if FIREBASE_CLOUD_MSG_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
	//! 토큰을 수신했을 경우
	private void OnReceiveToken(object a_oSender, TokenReceivedEventArgs a_oArgs) {
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_TOKEN_CALLBACK, () => {
			CFunc.ShowLog($"CFirebaseManager.OnReceiveToken: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);
			this.MsgToken = a_oArgs.Token;
		});	
	}

	//! 메세지를 수신했을 경우
	private void OnReceiveMsg(object a_oSender, MessageReceivedEventArgs a_oArgs) {
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_MSG_CALLBACK, () => {
			CFunc.ShowLog($"CFirebaseManager.OnReceiveMsg: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);
		});
	}
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
