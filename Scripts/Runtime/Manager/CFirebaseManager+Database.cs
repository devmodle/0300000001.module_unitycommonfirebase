using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE && FIREBASE_DATABASE_ENABLE
#if UNITY_IOS || UNITY_ANDROID
using Firebase.Database;
#endif			// #if #if UNITY_IOS || UNITY_ANDROID

//! 파이어 베이스 관리자 - 데이터 베이스
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 데이터를 저장한다
	public void SaveDatabase(List<string> a_oNodeList, 
		string a_oJSONString, System.Action<CFirebaseManager, bool> a_oCallback) 
	{
		CAccess.Assert(a_oNodeList.ExIsValid() && a_oJSONString.ExIsValid());
		CFunc.ShowLog("CFirebaseManager.SaveDatabase: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeList, a_oJSONString);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			m_oSaveDatabaseCallback = a_oCallback;
			var oDatabase = this.GetDatabase(a_oNodeList);

			CTaskManager.Instance.WaitAsyncTask(oDatabase.SetRawJsonValueAsync(a_oJSONString), 
				this.OnSaveDatabase);
		} else {
			a_oCallback?.Invoke(this, false);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 데이터를 로드한다
	public void LoadDatabase(List<string> a_oNodeList, System.Action<CFirebaseManager, string, bool> a_oCallback) {
		CAccess.Assert(a_oNodeList != null);
		CFunc.ShowLog("CFirebaseManager.LoadDatabase: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oNodeList);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			m_oLoadDatabaseCallback = a_oCallback;
			var oDatabase = this.GetDatabase(a_oNodeList);
			
			CTaskManager.Instance.WaitAsyncTask(oDatabase.GetValueAsync(), 
				this.OnLoadDatabase);
		} else {
			a_oCallback?.Invoke(this, string.Empty, false);
		}
#else
		a_oCallback?.Invoke(this, string.Empty, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 데이터 베이스를 반환한다
	private DatabaseReference GetDatabase(List<string> a_oNodeList) {
		var oRoot = FirebaseDatabase.DefaultInstance.RootReference;
		var oDatabase = oRoot;

		for(int i = 0; i < a_oNodeList.Count; ++i) {
			oDatabase = oDatabase.Child(a_oNodeList[i]);
		}

		return oDatabase;
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	//! 데이터를 저장했을 경우
	private void OnSaveDatabase(Task a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog("CFirebaseManager.OnSaveDatabase: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

		m_oSaveDatabaseCallback?.Invoke(this, a_oTask.ExIsComplete());
	}

	//! 데이터를 로드했을 경우
	private void OnLoadDatabase(Task<DataSnapshot> a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog("CFirebaseManager.OnLoadDatabase: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

		// 비동기 처리가 실패했을 경우
		if(!a_oTask.ExIsComplete()) {
			m_oLoadDatabaseCallback?.Invoke(this, string.Empty, false);
		} else {
			m_oLoadDatabaseCallback?.Invoke(this, a_oTask.Result.GetRawJsonValue(), true);
		}
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE && FIREBASE_DATABASE_ENABLE
