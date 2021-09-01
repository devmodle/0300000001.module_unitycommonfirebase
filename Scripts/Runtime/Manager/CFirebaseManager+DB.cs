using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_DB_ENABLE
using Firebase.Database;
#endif			// #if FIREBASE_DB_ENABLE

//! 파이어 베이스 관리자 - 데이터 베이스
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 데이터를 로드한다
	public void LoadDB(List<string> a_oNodeList, System.Action<CFirebaseManager, string, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.LoadDB: {a_oNodeList}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oNodeList != null);

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_DB_ENABLE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oLoadDBCallback = a_oCallback;
			var oDB = this.GetDB(a_oNodeList);
			
			CTaskManager.Inst.WaitAsyncTask(oDB.GetValueAsync(), this.OnLoadDB);
		} else {
			a_oCallback?.Invoke(this, string.Empty, false);
		}
#else
		a_oCallback?.Invoke(this, string.Empty, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_DB_ENABLE
	}

	//! 데이터를 저장한다
	public void SaveDB(List<string> a_oNodeList, string a_oJSONStr, System.Action<CFirebaseManager, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.SaveDB: {a_oNodeList}, {a_oJSONStr}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oNodeList != null && a_oJSONStr.ExIsValid());

#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_DB_ENABLE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oSaveDBCallback = a_oCallback;
			var oDB = this.GetDB(a_oNodeList);

			CTaskManager.Inst.WaitAsyncTask(oDB.SetRawJsonValueAsync(a_oJSONStr), this.OnSaveDB);
		} else {
			a_oCallback?.Invoke(this, false);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_DB_ENABLE
	}
	#endregion			// 함수

	#region 조건부 함수
#if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_DB_ENABLE
	//! 데이터가 로드 되었을 경우
	private void OnLoadDB(Task<DataSnapshot> a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog($"CFirebaseManager.OnLoadDB: {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_LOAD_DB_CALLBACK, () => {
			// 데이터가 로드 되었을 경우
			if(a_oTask.ExIsComplete()) {
				m_oLoadDBCallback?.Invoke(this, a_oTask.Result.GetRawJsonValue(), true);
			} else {
				m_oLoadDBCallback?.Invoke(this, string.Empty, false);
			}

			m_oLoadDBCallback = null;
		});
	}

	//! 데이터가 저장 되었을 경우
	private void OnSaveDB(Task a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog($"CFirebaseManager.OnSaveDB: {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FIREBASE_M_SAVE_DB_CALLBACK, () => {
			CFunc.Invoke(ref m_oSaveDBCallback, this, a_oTask.ExIsComplete());
		});
	}

	//! 데이터 베이스를 반환한다
	private DatabaseReference GetDB(List<string> a_oNodeList) {
		CAccess.Assert(a_oNodeList != null);
		var oDB = FirebaseDatabase.DefaultInstance.RootReference;

		for(int i = 0; i < a_oNodeList.Count; ++i) {
			// 노드가 유효 할 경우
			if(a_oNodeList[i].ExIsValid()) {
				oDB = oDB.Child(a_oNodeList[i]);
			}
		}
		
		return oDB.Child(this.UserID);
	}
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FIREBASE_DB_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
