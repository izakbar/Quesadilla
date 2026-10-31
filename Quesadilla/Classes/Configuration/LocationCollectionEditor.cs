using System;
using System.ComponentModel.Design;

namespace Quesadilla.Classes.Configuration
{

	public class LocationCollectionEditor : CollectionEditor
	{
		public static event EventHandler CollectionChanged;

		public LocationCollectionEditor(Type type)
			: base(type)
		{
		}

		protected override object SetItems(
			object editValue,
			object[] value)
		{
			var result = base.SetItems(editValue, value);

			CollectionChanged?.Invoke(this, EventArgs.Empty);

			return result;
		}
	}

}
