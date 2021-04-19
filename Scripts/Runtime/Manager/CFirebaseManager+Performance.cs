using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
//! 파이어 베이스 - 성능
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 추적을 시작한다
	public void StartTracking(string a_oName, Dictionary<string, string> a_oDataList) {
		CAccess.Assert(a_oName.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.StartTracking: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oName, a_oDataList);

#if FIREBASE_PERFORMANCE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
#if PERFORMANCE_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			var oDataList = a_oDataList ?? new Dictionary<string, string>();

			oDataList.ExAddVal(KCDefine.U_TRACKING_KEY_DEVICE_ID, CCommonAppInfoStorage.Inst.AppInfo.DeviceID);
			oDataList.ExAddVal(KCDefine.U_TRACKING_KEY_PLATFORM, CCommonAppInfoStorage.Inst.Platform);
			oDataList.ExAddVal(KCDefine.U_TRACKING_KEY_USER_TYPE, CCommonUserInfoStorage.Inst.UserInfo.UserType.ToString());

			CUnityMsgSender.Inst.SendTrackingMsg(a_oName, a_oDataList, true);
		}
#endif			// #if PERFORMANCE_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if FIREBASE_PERFORMANCE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 추적을 중지한다
	public void StopTracking(string a_oName) {
		CAccess.Assert(a_oName.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.StopTracking: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oName);

#if FIREBASE_PERFORMANCE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
#if PERFORMANCE_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			CUnityMsgSender.Inst.SendTrackingMsg(a_oName, null, false);
		}
#endif			// #if PERFORMANCE_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if FIREBASE_PERFORMANCE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
