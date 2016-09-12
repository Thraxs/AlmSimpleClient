using System.ComponentModel;
using System.Linq;
using Alphaleonis.Win32.Filesystem;

namespace ALMSimpleClient.OTA
{
    public class LabItem : INotifyPropertyChanged
    {
        //Basic properties
        public int Id { get; set; }
        public LabFolder Parent { get; set; }
        public LabItemType? Type { get; set; }
        public string Name { get; set; }

        //View properties
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Returns a valid Windows folder name for this item.
        /// </summary>
        /// <returns>The folder name.</returns>
        public string DirectoryName()
        {
            return Path.GetInvalidFileNameChars()
                .Aggregate(Name, (current, c) => current.Replace(c.ToString(), " "));
        }

        public virtual void CreateFolder(string path = "") {}
    }
}
