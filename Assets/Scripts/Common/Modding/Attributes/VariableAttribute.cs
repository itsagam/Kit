using System;
using System.Collections;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class VariableAttribute: Attribute
{
	public VariableAttribute()
	{
	}
}