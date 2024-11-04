using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AlgoVisualizer
{
    public class ArrayItem : INotifyPropertyChanged
    {
        private int _value;
        private Brush _indexColor = Brushes.Black;

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public Brush IndexColor
        {
            get => _indexColor;
            set
            {
                _indexColor = value;
                OnPropertyChanged();
            }
        }

        public ArrayItem Clone()
        {
            return new ArrayItem { Value = this.Value, IndexColor = this.IndexColor };
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
