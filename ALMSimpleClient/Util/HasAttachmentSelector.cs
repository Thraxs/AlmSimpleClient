using System.Windows;
using System.Windows.Controls;
using ALMSimpleClient.OTA;

namespace ALMSimpleClient.Util
{
    class HasAttachmentSelector : DataTemplateSelector
    {
        public DataTemplate TemplateHasAttachment { get; set; }
        public DataTemplate TemplateEmpty { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var obj = item as LabTestInstance;

            if (obj == null) return base.SelectTemplate(item, container);

            return obj.HasAttachments ? TemplateHasAttachment : TemplateEmpty;
        }
    }
}
