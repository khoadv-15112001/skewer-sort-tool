using DG.Tweening;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class PopupToast : Panel
{
	private static PopupToast popupToast;
	private readonly Service<PoolingContainerService> poolingContainer = new();
	
	[FoldoutGroup("Editor Test")]
	[SerializeField, LabelText("I2 Term")] 
	private string testTerm = "toast_test";
	
	[FoldoutGroup("Editor Test")]
	[SerializeField, LabelText("Param VALUE (optional)")] 
	private string testParam = "";
	
	[FoldoutGroup("Editor Test")]
	[Button("Test Toast", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
	private void TestToast()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("[PopupToast] Please enter Play Mode to test!");
			return;
		}
		
		Cretate(testTerm, string.IsNullOrEmpty(testParam) ? null : testParam);
		Debug.Log($"[PopupToast] Testing toast with term: {testTerm}, param: {testParam}");
	}
	
	public static void Cretate(string content, string param = null)
	{

		if (popupToast == null || !popupToast.gameObject.activeInHierarchy)
		{
			UIData uIData = new UIData();
			uIData.Add("content", content);
			uIData.Add("param", param);
			popupToast = PanelManager.Instance.OpenPanel<PopupToast>(uIData);
		}
		else
		{
			popupToast.transform.SetAsLastSibling();
			popupToast.AddToast(content, param);
		}
	}

	public Transform container;
	public override void Open(UIData uIData)
	{
		base.Open(uIData);
		poolingContainer.Instance.CleanContainer(container);
		string content = uiData.Get<string>("content");
		string param = uiData.Get<string>("param");
		AddToast(content, param);
	}


	public void AddToast(string content, string param = null)
	{
		UIToastItem toast = poolingContainer.Instance.CreateObject<UIToastItem>(container);
		toast.SetData(content, param);
		
		float totalDuration = toast.TotalDuration;
		DOVirtual.DelayedCall(totalDuration, () => toast.gameObject.SetActive(false));
		DOTween.Kill("toast_dismiss");
		DOVirtual.DelayedCall(totalDuration + 0.2f, () => Close()).SetId("toast_dismiss");
	}
}
