using System.Windows;
using System.Windows.Controls;
using ALMSimpleClient.OTA;

namespace ALMSimpleClient.Util
{
    class LabRunStatusSelector : DataTemplateSelector
    {
        public DataTemplate TemplateBlocked { get; set; }
        public DataTemplate TemplateFailed { get; set; }
        public DataTemplate TemplateNotAvailable { get; set; }
        public DataTemplate TemplateNoRun { get; set; }
        public DataTemplate TemplateNotCompleted { get; set; }
        public DataTemplate TemplatePassed { get; set; }
        public DataTemplate TemplateOther { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var obj = item as LabTestInstance;

            if (obj == null) return base.SelectTemplate(item, container);

            switch (obj.LabRunStatus)
            {
                case LabRunStatus.Blocked:
                    return TemplateBlocked;
                case LabRunStatus.Failed:
                    return TemplateFailed;
                case LabRunStatus.NotAvailable:
                    return TemplateNotAvailable;
                case LabRunStatus.NoRun:
                    return TemplateNoRun;
                case LabRunStatus.NotCompleted:
                    return TemplateNotCompleted;
                case LabRunStatus.Passed:
                    return TemplatePassed;
                default:
                    return TemplateOther;
            }
        }
    }
}
