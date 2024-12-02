using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AlgoVisualizer.Algorithms
{
    public class Dijkstra : UserControl
    {
        public Dijkstra() => Content = new CommonPageUserControl(isGraph: true) { ParentIdentifier = "Dijkstra" };
    }
}
