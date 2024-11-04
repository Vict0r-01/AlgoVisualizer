using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AlgoVisualizer.Algorithms
{
    public class LinearSearch : UserControl
    {
        public LinearSearch() => Content = new CommonPageUserControl() { ParentIdentifier = "SelectionSort" };
    }
}
