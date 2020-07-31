using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ENABLE && FIREBASE_ANALYTICS_ENABLE
using Firebase.Analytics;

//! 파이어 베이스 관리자 - 분석
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 분석 유저 식별자를 변경한다
	public void SetAnalyticsUserID(string a_oID) {
		CAccess.Assert(a_oID.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.SetAnalyticsUserID: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oID);

		if(this.IsInit) {
			FirebaseAnalytics.SetUserId(a_oID);
		}
	}

	//! 분석 데이터를 변경한다
	public void SetAnalyticsDatas(Dictionary<string, string> a_oDataList) {
		CAccess.Assert(a_oDataList.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.SetAnalyticsDatas: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oDataList);

		if(this.IsInit) {
			foreach(var stKeyValue in a_oDataList) {
				FirebaseAnalytics.SetUserProperty(stKeyValue.Key, stKeyValue.Value);
			}
		}
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, string a_oParam) {
		this.SendLog(a_oName, a_oParam, null);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, string a_oParam, List<string> a_oDataList) {
		CAccess.Assert(a_oName.ExIsValid() && a_oParam.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.SendLog: {0}, {1}, {2}", KCDefine.B_LOG_COLOR_PLUGIN, a_oName, a_oParam, a_oDataList);

#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		if(this.IsInit) {
			var oDataList = a_oDataList ?? new List<string>();

#if MSG_PACK_ENABLE
			oDataList.ExAddValue(CAppInfoStorage.Instance.AppInfo.DeviceID);

#if AUTO_LOG_PARAM_ENABLE
			oDataList.ExAddValue(CAppInfoStorage.Instance.PlatformName);
			oDataList.ExAddValue(CUserInfoStorage.Instance.UserInfo.UserType.ToString());

			oDataList.ExAddValue(System.DateTime.UtcNow.ExToLongString());
			oDataList.ExAddValue(CAppInfoStorage.Instance.AppInfo.UTCInstallTime.ExToLongString());
#endif			// #if AUTO_LOG_PARAM_ENABLE
#endif			// #if MSG_PACK_ENABLE

			string oLog = oDataList.ExToString(KCDefine.B_TOKEN_CSV_STRING);
			FirebaseAnalytics.LogEvent(a_oName, a_oParam, oLog);
		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
	}
	#endregion			// 함수
}
#endif			// #if FIREBASE_ENABLE && FIREBASE_ANALYTICS_ENABLE
