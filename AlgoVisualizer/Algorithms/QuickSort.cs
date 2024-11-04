using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AlgoVisualizer.Algorithms
{
    public class QuickSort : UserControl
    {
        public QuickSort() => Content = new CommonPageUserControl() { ParentIdentifier = "QuickSort" };
    }
}
