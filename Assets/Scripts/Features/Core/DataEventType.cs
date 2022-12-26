using System;

[Flags]
public enum DataEventType
{
	None    = 0,
	Load    = 1 << 0,
	Add     = 1 << 1,
	Remove  = 1 << 2,
	Change  = 1 << 3,
	Reorder = 1 << 4,
	Update  = Add | Remove,
}
