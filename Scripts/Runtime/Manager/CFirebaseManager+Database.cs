using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE && FIREBASE_DATABASE_ENABLE
using Firebase.Database;

//! 파이어 베이스 관리자 - 데이터 베이스
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 데이터를 저장한다
	public void SaveData(List<string> a_oNodeNameList, 
		string a_oJSONString, System.Action<CFirebaseManager, bool> a_oCallback) 
	{
		CAccess.Assert(a_oJSONString.ExIsValid() && a_oNodeNameList != null);
		CFunc.ShowLog("CFirebaseManager.SaveData: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeNameList, a_oJSONString);

		// 초기화가 필요 할 경우
		if(!this.IsInit) {
			a_oCallback?.Invoke(this, false);
		} else {
			var oRootRef = FirebaseDatabase.DefaultInstance.RootReference;
			var oDatabaseRef = oRootRef;

			for(int i = 0; i < a_oNodeNameList.Count; ++i) {
				oDatabaseRef = oDatabaseRef.Child(a_oNodeNameList[i]);
			}

			CTaskManager.Instance.WaitAsyncTask(oDatabaseRef.SetRawJsonValueAsync(a_oJSONString), (a_oTask) => {
				string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
				CFunc.ShowLog("CFirebaseManager.OnSaveData: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

				a_oCallback?.Invoke(this, a_oTask.ExIsComplete());
			});
		}
	}

	//! 데이터를 로드한다
	public void LoadData(List<string> a_oNodeNameList, System.Action<CFirebaseManager, string, bool> a_oCallback) {
		CAccess.Assert(a_oNodeNameList != null);
		CFunc.ShowLog("CFirebaseManager.LoadData: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeNameList);

		// 초기화가 필요 할 경우
		if(!this.IsInit) {
			a_oCallback?.Invoke(this, string.Empty, false);
		} else {
			var oRootRef = FirebaseDatabase.DefaultInstance.RootReference;
			var oDatabaseRef = oRootRef;

			for(int i = 0; i < a_oNodeNameList.Count; ++i) {
				oDatabaseRef = oDatabaseRef.Child(a_oNodeNameList[i]);
			}

			CTaskManager.Instance.WaitAsyncTask(oDatabaseRef.GetValueAsync(), (a_oTask) => {
				var oTask = a_oTask as Task<DataSnapshot>;
				string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;

				CFunc.ShowLog("CFirebaseManager.OnLoadData: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

				// 비동기 처리가 실패했을 경우
				if(!a_oTask.ExIsComplete()) {
					a_oCallback?.Invoke(this, string.Empty, false);
				} else {
					a_oCallback?.Invoke(this, oTask.Result.GetRawJsonValue(), true);
				}
			});
		}
	}
	#endregion			// 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE && FIREBASE_DATABASE_ENABLE
