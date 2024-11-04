using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AlgoVisualizer.Algorithms
{
    public class FibonacciSearch : UserControl
    {
        public FibonacciSearch() => Content = new CommonPageUserControl() { ParentIdentifier = "FibonacciSearch" };
    }
}
