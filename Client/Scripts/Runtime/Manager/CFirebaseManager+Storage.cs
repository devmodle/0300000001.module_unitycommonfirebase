using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if FIREBASE_MODULE_ENABLE
using System.IO;
using System.Threading.Tasks;

#if FIREBASE_STORAGE_ENABLE
using Firebase.Storage;
#endif // #if FIREBASE_STORAGE_ENABLE

/** 파이어 베이스 관리자 - 저장소 */
public partial class CFirebaseManager : CSingleton<CFirebaseManager> {
	#region 함수
	/** 파일을 로드한다 */
	public void LoadFiles(string a_oPathFile, System.Action<CFirebaseManager, string, bool> a_oCallback) {
		CFunc.ShowLog($"CFirebaseManager.LoadDatas: {a_oPathFile}", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_oPathFile.ExIsValid());

#if FIREBASE_STORAGE_ENABLE && (UNITY_IOS || UNITY_ANDROID)
		// 파일 로드가 불가능 할 경우
		if(!this.IsInit) {
			goto FIREBASE_MANAGER_LOAD_FILES_EXIT;
		}

		m_oCallbackDictB.ExReplaceVal(EFirebaseCallback.LOAD_FILES, a_oCallback);
		var oFirebaseStorage = FirebaseStorage.DefaultInstance.GetReference(a_oPathFile);

		CManagerTask.Inst.WaitAsyncTask(oFirebaseStorage.GetStreamAsync(), this.OnLoadFiles);
		return;
#else
		CFunc.Invoke(ref a_oCallback, this, string.Empty, false);
#endif // #if FIREBASE_STORAGE_ENABLE && (UNITY_IOS || UNITY_ANDROID)

FIREBASE_MANAGER_LOAD_FILES_EXIT:
		CFunc.Invoke(ref a_oCallback, this, string.Empty, false);
	}

#if FIREBASE_STORAGE_ENABLE
	/** 파일이 로드되었을 경우 */
	public void OnLoadFiles(Task<Stream> a_oTask) {
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;
		CFunc.ShowLog($"CFirebaseManager.OnLoadFiles: {oErrorMsg}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.B_KEY_FIREBASE_M_LOAD_FILES_CALLBACK, () => {
			bool bIsSuccess = a_oTask.ExIsCompleteSuccess();
			string oResultStr = bIsSuccess ? CFunc.ReadStr(a_oTask.Result, true) : string.Empty;

			m_oCallbackDictB.GetValueOrDefault(EFirebaseCallback.LOAD_FILES)?.Invoke(this, oResultStr, bIsSuccess);
		});
	}
#endif // #if FIREBASE_STORAGE_ENABLE
#endregion // 함수
}
#endif // #if FIREBASE_MODULE_ENABLE
