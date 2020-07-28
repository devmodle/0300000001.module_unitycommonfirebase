using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ENABLE && FIREBASE_DATABASE_ENABLE
using Firebase.Database;

//! 파이어 베이스 관리자 - 데이터 베이스
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 데이터를 저장한다
	public void SaveData(List<string> a_oNodeNameList, string a_oJSONString, System.Action<CFirebaseManager, bool> a_oCallback) {
		CAccess.Assert(a_oJSONString.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.SaveData: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeNameList, a_oJSONString);

		if(!this.IsInit) {
			a_oCallback?.Invoke(this, false);
		} else {
			var oRootReference = FirebaseDatabase.DefaultInstance.RootReference;
			var oDatabaseReference = oRootReference;

			for(int i = 0; i < a_oNodeNameList?.Count; ++i) {
				oDatabaseReference = oDatabaseReference.Child(a_oNodeNameList[i]);
			}

			CFunc.WaitAsyncTask(oDatabaseReference.SetRawJsonValueAsync(a_oJSONString), (a_oTask) => {
				CFunc.ShowLog("CFirebaseManager.OnSaveData: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oTask.Exception?.Message);
				a_oCallback?.Invoke(this, a_oTask.ExIsComplete());
			});
		}
	}

	//! 데이터를 로드한다
	public void LoadData(List<string> a_oNodeNameList, System.Action<CFirebaseManager, string, bool> a_oCallback) {
		CFunc.ShowLog("CFirebaseManager.LoadData: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeNameList);

		if(!this.IsInit) {
			a_oCallback?.Invoke(this, string.Empty, false);
		} else {
			var oRootReference = FirebaseDatabase.DefaultInstance.RootReference;
			var oDatabaseReference = oRootReference;

			for(int i = 0; i < a_oNodeNameList?.Count; ++i) {
				oDatabaseReference = oDatabaseReference.Child(a_oNodeNameList[i]);
			}

			CFunc.WaitAsyncTask(oDatabaseReference.GetValueAsync(), (a_oTask) => {
				CFunc.ShowLog("CFirebaseManager.OnLoadData: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oTask.Exception?.Message);

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
#endif			// #if FIREBASE_ENABLE && FIREBASE_DATABASE_ENABLE
