using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AlgoVisualizer
{
    public interface IItem
    {
        int Value { get; set; }
        Brush IndexColor { get; set; }

    }
}
