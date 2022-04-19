using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if FIREBASE_MODULE_ENABLE 
#if FIREBASE_CLOUD_MSG_ENABLE
using Firebase.Messaging;
#endif			// #if FIREBASE_CLOUD_MSG_ENABLE

/** 파이어 베이스 관리자 - 메세지 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 조건부 함수
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CLOUD_MSG_ENABLE
	/** 메세지 토큰을 수신했을 경우 */
	private void OnReceiveMsgToken(object a_oSender, TokenReceivedEventArgs a_oArgs) {
		CFunc.ShowLog($"CFirebaseManager.OnReceiveMsgToken: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_TOKEN_CALLBACK, () => {
			this.MsgToken = a_oArgs.Token;
			CCommonAppInfoStorage.Inst.AppInfo.FirebaseMsgToken = a_oArgs.Token;
		});
	}

	/** 알림 메세지를 수신했을 경우 */
	private void OnReceiveNotiMsg(object a_oSender, MessageReceivedEventArgs a_oArgs) {
		CFunc.ShowLog($"CFirebaseManager.OnReceiveNotiMsg: {a_oArgs}", KCDefine.B_LOG_COLOR_PLUGIN);
		
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_NOTI_MSG_CALLBACK, () => {
			// Do Something
		});
	}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_CLOUD_MSG_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
