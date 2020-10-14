using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
#if FIREBASE_FIRESTORE_ENABLE
using Firebase.Firestore;
#endif			// #if FIREBASE_FIRESTORE_ENABLE

//! 파이어 베이스 관리자 - 파이어 스토어
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	//! 데이터를 저장한다
	public void SaveFirestore(List<string> a_oCollectionList, 
		List<string> a_oDocList, string a_oJSONString, System.Action<CFirebaseManager, bool> a_oCallback) 
	{
		CAccess.Assert(a_oJSONString.ExIsValid());

		CFunc.ShowLog("CFirebaseManager.SaveFirestore: {0}, {1}, {2}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oCollectionList, a_oDocList, a_oJSONString);

#if FIREBASE_FIRESTORE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			m_oSaveFirestoreCallback = a_oCallback;
			var oDoc = this.GetDoc(a_oCollectionList, a_oDocList);

			CTaskManager.Instance.WaitAsyncTask(oDoc.SetAsync(a_oJSONString), this.OnSaveFirestore);
		} else {
			a_oCallback?.Invoke(this, false);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if FIREBASE_FIRESTORE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}

	//! 데이터를 로드한다
	public void LoadFirestore(List<string> a_oCollectionList, 
		List<string> a_oDocList, System.Action<CFirebaseManager, string, bool> a_oCallback) 
	{
		CFunc.ShowLog("CFirebaseManager.LoadFirestore: {0}, {1}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oCollectionList, a_oDocList);

#if FIREBASE_FIRESTORE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			m_oLoadFirestoreCallback = a_oCallback;
			var oDoc = this.GetDoc(a_oCollectionList, a_oDocList);

			CTaskManager.Instance.WaitAsyncTask(oDoc.GetSnapshotAsync(), this.OnLoadFirestore);
		} else {
			a_oCallback?.Invoke(this, string.Empty, false);
		}
#else
		a_oCallback?.Invoke(this, string.Empty, false);
#endif			// #if FIREBASE_FIRESTORE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수

	#region 조건부 함수
#if FIREBASE_FIRESTORE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	//! 데이터를 저장했을 경우
	private void OnSaveFirestore(Task a_oTask) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FIREBASE_M_SAVE_FIRESTORE_CALLBACK, () => {
			string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
			CFunc.ShowLog("CFirebaseManager.OnSaveFirestore: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

			m_oSaveFirestoreCallback?.Invoke(this, a_oTask.ExIsComplete());
		});
	}

	//! 데이터를 로드했을 경우
	private void OnLoadFirestore(Task<DocumentSnapshot> a_oTask) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FIREBASE_M_LOAD_FIRESTORE_CALLBACK, () => {
			string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
			CFunc.ShowLog("CFirebaseManager.OnLoadFirestore: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

			// 비동기 처리가 실패했을 경우
			if(!a_oTask.ExIsComplete()) {
				m_oLoadFirestoreCallback?.Invoke(this, string.Empty, false);
			} else {
				m_oLoadFirestoreCallback?.Invoke(this, a_oTask.Result.ToString(), true);
			}
		});
	}

	//! 문서를 반환한다
	private DocumentReference GetDoc(List<string> a_oCollectionList, List<string> a_oDocList) {
		CAccess.Assert(a_oCollectionList.ExIsValid() && 
			a_oDocList.ExIsValid() && a_oCollectionList.Count == a_oDocList.Count);

		var oCollection = FirebaseFirestore.DefaultInstance.Collection(a_oCollectionList[KCDefine.B_INDEX_START]);
		var oDoc = oCollection.Document(a_oDocList[KCDefine.B_INDEX_START]);

		for(int i = KCDefine.B_INDEX_START + 1; i < a_oDocList.Count; ++i) {
			oCollection = oDoc.Collection(a_oCollectionList[i]);
			oDoc = oCollection.Document(a_oDocList[i]);
		}

		return oDoc;
	}
#endif			// #if FIREBASE_FIRESTORE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
	#endregion			// 조건부 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE
