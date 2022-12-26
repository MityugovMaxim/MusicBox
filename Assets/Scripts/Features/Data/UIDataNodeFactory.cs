using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIDataNodeFactory
{
	[Inject] UIDataObject.Pool     m_ObjectPool;
	[Inject] UIDataBoolean.Pool    m_BooleanPool;
	[Inject] UIDataField.Pool      m_FieldPool;
	[Inject] UIDataArea.Pool       m_AreaPool;
	[Inject] UIDataDate.Pool       m_DatePool;
	[Inject] UIDataTick.Pool       m_TickPool;
	[Inject] UIDataEnum.Pool       m_EnumPool;
	[Inject] UIDataSlider.Pool     m_SliderPool;
	[Inject] UIDataCollection.Pool m_CollectionPool;

	public UIDataEntity Create(DataNode _DataNode, RectTransform _Container)
	{
		if (_DataNode.HasAttribute<DataHideAttribute>())
			return null;
		
		if (IsField(_DataNode.Type))
		{
			if (_DataNode.HasAttribute<DataSliderAttribute>())
				return m_SliderPool.Spawn(_Container, _DataNode);
			if (_DataNode.HasAttribute<DataAreaAttribute>())
				return m_AreaPool.Spawn(_Container, _DataNode);
			if (_DataNode.HasAttribute<DataDateAttribute>())
				return m_DatePool.Spawn(_Container, _DataNode);
			if (_DataNode.HasAttribute<DataTickAttribute>())
				return m_TickPool.Spawn(_Container, _DataNode);
			return m_FieldPool.Spawn(_Container, _DataNode);
		}
		if (IsBoolean(_DataNode.Type))
			return m_BooleanPool.Spawn(_Container, _DataNode);
		if (IsEnum(_DataNode.Type))
			return m_EnumPool.Spawn(_Container, _DataNode);
		if (IsObject(_DataNode.Type))
			return m_ObjectPool.Spawn(_Container, _DataNode);
		if (IsCollection(_DataNode.Type))
			return m_CollectionPool.Spawn(_Container, _DataNode);
		return m_FieldPool.Spawn(_Container, _DataNode);
	}

	public void Remove(UIDataEntity _DataEntity)
	{
		if (_DataEntity is UIDataObject objectItem)
			m_ObjectPool.Despawn(objectItem);
		else if (_DataEntity is UIDataField fieldItem)
			m_FieldPool.Despawn(fieldItem);
		else if (_DataEntity is UIDataArea areaItem)
			m_AreaPool.Despawn(areaItem);
		else if (_DataEntity is UIDataDate dateItem)
			m_DatePool.Despawn(dateItem);
		else if (_DataEntity is UIDataTick tickItem)
			m_TickPool.Despawn(tickItem);
		else if (_DataEntity is UIDataEnum enumItem)
			m_EnumPool.Despawn(enumItem);
		else if (_DataEntity is UIDataSlider sliderItem)
			m_SliderPool.Despawn(sliderItem);
		else if (_DataEntity is UIDataCollection collectionItem)
			m_CollectionPool.Despawn(collectionItem);
	}

	bool IsObject(Type _Type)
	{
		return _Type.IsClass && !_Type.IsAbstract && !_Type.IsGenericType;
	}

	bool IsField(Type _Type)
	{
		return _Type == typeof(int) ||
			_Type == typeof(long) ||
			_Type == typeof(float) ||
			_Type == typeof(double) ||
			_Type == typeof(decimal) ||
			_Type == typeof(string);
	}

	bool IsBoolean(Type _Type)
	{
		return _Type == typeof(bool);
	}

	bool IsEnum(Type _Type)
	{
		return _Type.IsEnum;
	}

	bool IsCollection(Type _Type)
	{
		if (_Type.IsArray)
			return true;
		
		if (_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(List<>))
			return true;
		
		if (_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			return true;
		
		return false;
	}
}
