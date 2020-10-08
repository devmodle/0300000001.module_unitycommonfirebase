using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE && FIREBASE_FIRESTORE_ENABLE
#if UNITY_IOS || UNITY_ANDROID
using Firebase.Firestore;
#endif			// #if UNITY_IOS || UNITY_ANDROID

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

		// 초기화가 필요 할 경우
		if(!this.IsInit) {
			a_oCallback?.Invoke(this, false);
		} else {
#if UNITY_IOS || UNITY_ANDROID
			var oDoc = this.GetDoc(a_oCollectionList, a_oDocList);	

			CTaskManager.Instance.WaitAsyncTask(oDoc.SetAsync(a_oJSONString), (a_oTask) => {
				string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
				CFunc.ShowLog("CFirebaseManager.OnSaveFirestore: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

				a_oCallback?.Invoke(this, a_oTask.ExIsComplete());
			});
#else
			a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}
	}

	//! 데이터를 로드한다
	public void LoadFirestore(List<string> a_oCollectionList, 
		List<string> a_oDocList, System.Action<CFirebaseManager, string, bool> a_oCallback) 
	{
		CFunc.ShowLog("CFirebaseManager.LoadFirestore: {0}, {1}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oCollectionList, a_oDocList);

		// 초기화가 필요 할 경우
		if(!this.IsInit) {
			a_oCallback?.Invoke(this, string.Empty, false);
		} else {
#if UNITY_IOS || UNITY_ANDROID
			var oDoc = this.GetDoc(a_oCollectionList, a_oDocList);

			CTaskManager.Instance.WaitAsyncTask(oDoc.GetSnapshotAsync(), (a_oTask) => {
				string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
				CFunc.ShowLog("CFirebaseManager.OnLoadFirestore: {0}", KCDefine.B_LOG_COLOR_PLUGIN, oErrorMsg);

				// 비동기 처리가 실패했을 경우
				if(!a_oTask.ExIsComplete()) {
					a_oCallback?.Invoke(this, string.Empty, false);
				} else {
					a_oCallback?.Invoke(this, a_oTask.Result.ToString(), true);
				}
			});
#else
			a_oCallback?.Invoke(this, string.Empty, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}
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
	#endregion			// 함수
}
#endif			// #if FIREBASE_MODULE_ENABLE && FIREBASE_FIRESTORE_ENABLE
