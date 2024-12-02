using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AlgoVisualizer
{
    public class GraphItem : ArrayItem, INotifyPropertyChanged, IItem
    {
        private double _x; private double _y;

        public ObservableCollection<Line> Edge = new ObservableCollection<Line>();

        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Line> DeepCopy(ObservableCollection<Line> original, GraphItem graphItem)
        {
            // Ensure the original collection is not null
            if (original == null)
                return new ObservableCollection<Line>();

            return new ObservableCollection<Line>(original.Select(item =>
            {
                var line = new Line
                {
                    X1 = item.X1,
                    Y1 = item.Y1,
                    X2 = item.X2,
                    Y2 = item.Y2,
                    StrokeThickness = 2
                };

                // Create the binding to the GraphItem's IndexColor
                var binding = new Binding("IndexColor")
                {
                    Source = graphItem, // Bind to the GraphItem, not the collection
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(line, Line.StrokeProperty, binding);

                return line;
            }));
        }
        public new GraphItem Clone()
        {
            var clone = new GraphItem
            {
                Value = this.Value,
                X = this.X,
                Y = this.Y,
                IndexColor = this.IndexColor,
                Edge = new ObservableCollection<Line>()
            };

            foreach (var line in this.Edge)
            {
                // Reapply the binding to the Stroke property
                var binding = new Binding("IndexColor")
                {
                    Source = clone, // Bind to the clone, not the original
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(line, Line.StrokeProperty, binding);

                // Add the cloned and re-bound line to the clone's Edge collection
                clone.Edge.Add(line);
            }

            return clone;
        }
    }
}
