using Quesadilla.Classes.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
public class CustomPropertyExtractorWrapper : CustomTypeDescriptor
{
    public object WrappedObject { get; private set; }
    public CustomPropertyExtractorWrapper(object o)
        : base(TypeDescriptor.GetProvider(o).GetTypeDescriptor(o))
    {
        WrappedObject = o;
    }
    public override PropertyDescriptorCollection GetProperties()
    {
        return this.GetProperties(new Attribute[] { });
    }
    public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    {
        var properties = base.GetProperties().Cast<PropertyDescriptor>()
            .Where(o => o.PropertyType == typeof(Color) || 
            o.PropertyType == typeof(bool) || 
            o.PropertyType == typeof(int) || 
            o.PropertyType == typeof(PopupLocationType) ||
            o.Category == "HeartBeat"
            )
            .ToArray();
        return new PropertyDescriptorCollection(properties);
    }
}