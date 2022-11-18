using System;

[Flags]
public enum DataEventType
{
	None    = 0,
	Add     = 1 << 0,
	Remove  = 1 << 1,
	Change  = 1 << 2,
	Reorder = 1 << 3,
	Update  = Add | Remove,
}
