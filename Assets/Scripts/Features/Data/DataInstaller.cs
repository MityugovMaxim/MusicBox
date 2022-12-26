using UnityEngine;

public class DataInstaller : FeatureInstaller
{
	[SerializeField] UIDataObject     m_DataObject;
	[SerializeField] UIDataField      m_DataField;
	[SerializeField] UIDataArea       m_DataArea;
	[SerializeField] UIDataDate       m_DataDate;
	[SerializeField] UIDataTick       m_DataTick;
	[SerializeField] UIDataSlider     m_DataSlider;
	[SerializeField] UIDataEnum       m_DataEnum;
	[SerializeField] UIDataBoolean    m_DataBoolean;
	[SerializeField] UIDataCollection m_DataCollection;

	public override void InstallBindings()
	{
		InstallSingleton<UIDataNodeFactory>();
		
		InstallPool<UIDataObject, UIDataObject.Pool>(m_DataObject, 0);
		InstallPool<UIDataField, UIDataField.Pool>(m_DataField, 0);
		InstallPool<UIDataArea, UIDataArea.Pool>(m_DataArea, 0);
		InstallPool<UIDataDate, UIDataDate.Pool>(m_DataDate, 0);
		InstallPool<UIDataTick, UIDataTick.Pool>(m_DataTick, 0);
		InstallPool<UIDataSlider, UIDataSlider.Pool>(m_DataSlider, 0);
		InstallPool<UIDataEnum, UIDataEnum.Pool>(m_DataEnum, 0);
		InstallPool<UIDataBoolean, UIDataBoolean.Pool>(m_DataBoolean, 0);
		InstallPool<UIDataCollection, UIDataCollection.Pool>(m_DataCollection, 0);
	}
}
