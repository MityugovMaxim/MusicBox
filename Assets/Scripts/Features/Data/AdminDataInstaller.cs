using UnityEngine;

public class AdminDataInstaller : FeatureInstaller
{
	[SerializeField] UIAdminObjectNode  m_ObjectNode;
	[SerializeField] UIAdminNumberNode  m_NumberNode;
	[SerializeField] UIAdminStringNode  m_StringNode;
	[SerializeField] UIAdminBooleanNode m_BooleanNode;
	[SerializeField] UIAdminArrayNode   m_ArrayNode;
	[SerializeField] UIAdminSliderNode  m_SliderNode;
	[SerializeField] UIAdminDateNode    m_DateNode;
	[SerializeField] UIAdminTimeNode    m_TimeNode;
	[SerializeField] UIAdminTickNode    m_TickNode;
	[SerializeField] UIAdminEnumNode    m_EnumNode;
	[SerializeField] UIAdminSearchNode  m_SearchNode;

	public override void InstallBindings()
	{
		InstallSingleton<UIAdminNodeFactory>();
		
		InstallPool<UIAdminObjectNode, UIAdminObjectNode.Pool>(m_ObjectNode, 0);
		InstallPool<UIAdminStringNode, UIAdminStringNode.Pool>(m_StringNode, 0);
		InstallPool<UIAdminNumberNode, UIAdminNumberNode.Pool>(m_NumberNode, 0);
		InstallPool<UIAdminBooleanNode, UIAdminBooleanNode.Pool>(m_BooleanNode, 0);
		InstallPool<UIAdminArrayNode, UIAdminArrayNode.Pool>(m_ArrayNode, 0);
		InstallPool<UIAdminSliderNode, UIAdminSliderNode.Pool>(m_SliderNode, 0);
		InstallPool<UIAdminDateNode, UIAdminDateNode.Pool>(m_DateNode, 0);
		InstallPool<UIAdminTimeNode, UIAdminTimeNode.Pool>(m_TimeNode, 0);
		InstallPool<UIAdminTickNode, UIAdminTickNode.Pool>(m_TickNode, 0);
		InstallPool<UIAdminEnumNode, UIAdminEnumNode.Pool>(m_EnumNode, 0);
		InstallPool<UIAdminSearchNode, UIAdminSearchNode.Pool>(m_SearchNode, 0);
	}
}
