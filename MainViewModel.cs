using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PolygonEditor
{
    public class MainViewModel: INotifyPropertyChanged
    {
        private WriteableBitmap _polygonBitmap;
        public WriteableBitmap PolygonBitmap
        {
            get { return _polygonBitmap; }
            set { SetField(ref _polygonBitmap, value, "PolygonBitmap"); }
        }

        public MainViewModel()
        {
        }

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
