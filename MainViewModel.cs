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
    public enum States
    {
        PolygonAdd, Edit
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private WriteableBitmap _polygonBitmap;
        public WriteableBitmap PolygonBitmap
        {
            get { return _polygonBitmap; }
            set { SetField(ref _polygonBitmap, value, "PolygonBitmap"); }
        }

        string _state;
        public string ActualState
        {
            get { return _state; }
            set { SetField(ref _state, value, "ActualState"); }
        }

        States _st;
        public States state
        {
            get { return _st; }
            set
            {
                _st = value;
                switch (value)
                {
                    case States.Edit:
                        ActualState = "Tryb Edycji";
                        break;
                    case States.PolygonAdd:
                        ActualState = "Tryb dodawania wielokąta";
                        break;
                }

            }
        }

        public MainViewModel()
        {
            ActualState = "Dodaj pierwszy wielokąt";
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
