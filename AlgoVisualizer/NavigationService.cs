using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AlgoVisualizer
{
    internal class NavigationService : INavigationService
    {
        private readonly Action<object> _navigationAction;
        private readonly Dictionary<string, object> _pageCache = new();
        public NavigationService(Action<object> navigationAction)
        {
            _navigationAction = navigationAction;
        }

        public void NavigateTo<T>() where T : class
        {
            string pageKey = typeof(T).FullName;
            if (!_pageCache.ContainsKey(pageKey))
            {
                _pageCache[pageKey] = Activator.CreateInstance(typeof(T));
            }

            _navigationAction.Invoke(_pageCache[pageKey]);
        }
    }
}
