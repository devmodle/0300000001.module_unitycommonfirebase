using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE && FIREBASE_DATABASE_ENABLE
using Firebase.Database;

//! 파이어 베이스 관리자 - 데이터 베이스
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 데이터 베이스를 저장한다
	public void SaveDatabase(List<string> a_oNodeNameList, string a_oJSONString, System.Action<CFirebaseManager, bool> a_oCallback) {
		CAccess.Assert(a_oJSONString.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.SaveDatabase: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeNameList, a_oJSONString);

		if(!this.IsInit) {
			a_oCallback?.Invoke(this, false);
		} else {
			var oRootRef = FirebaseDatabase.DefaultInstance.RootReference;
			var oDatabaseRef = oRootRef;

			for(int i = 0; i < a_oNodeNameList?.Count; ++i) {
				oDatabaseRef = oDatabaseRef.Child(a_oNodeNameList[i]);
			}

			CTaskManager.Instance.WaitAsyncTask(oDatabaseRef.SetRawJsonValueAsync(a_oJSONString), (a_oTask) => {
				CFunc.ShowLog("CFirebaseManager.OnSaveDatabase: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oTask.Exception?.Message);
				a_oCallback?.Invoke(this, a_oTask.ExIsComplete());
			});
		}
	}

	//! 데이터 베이스를 로드한다
	public void LoadDatabase(List<string> a_oNodeNameList, System.Action<CFirebaseManager, string, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoadDatabase: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeNameList);

		if(!this.IsInit) {
			a_oCallback?.Invoke(this, string.Empty, false);
		} else {
			var oRootRef = FirebaseDatabase.DefaultInstance.RootReference;
			var oDatabaseRef = oRootRef;

			for(int i = 0; i < a_oNodeNameList?.Count; ++i) {
				oDatabaseRef = oDatabaseRef.Child(a_oNodeNameList[i]);
			}

			CTaskManager.Instance.WaitAsyncTask(oDatabaseRef.GetValueAsync(), (a_oTask) => {
				CFunc.ShowLog("CFirebaseManager.OnLoadDatabase: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oTask.Exception?.Message);

				if(!a_oTask.ExIsComplete()) {
					a_oCallback?.Invoke(this, string.Empty, false);
				} else {
					a_oCallback?.Invoke(this, a_oTask.Result.GetRawJsonValue(), true);
				}
			});
		}
	}
	#endregion			// 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE && FIREBASE_DATABASE_ENABLE
