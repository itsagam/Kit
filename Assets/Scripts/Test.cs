using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Game
{
	public class Test: MonoBehaviour
	{
		[ValueDropdown("GetAllValues", FlattenTreeView = true)]
		public string Value;

		private void Awake()
		{
			print(typeof(Windows).GetField(Value).GetValue(null));
		}

		public IEnumerable<ValueDropdownItem<string>> GetAllValues()
		{
			var fields = typeof(Windows).GetFields(BindingFlags.Public | BindingFlags.Static);
			var strings = fields.Where(f => f.FieldType == typeof(string));
			return strings.Select(s => new ValueDropdownItem<string>(ObjectNames.NicifyVariableName(s.Name), s.Name));
		}
	}
}