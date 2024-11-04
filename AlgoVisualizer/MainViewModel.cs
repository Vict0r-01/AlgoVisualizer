using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AlgoVisualizer.Algorithms;

namespace AlgoVisualizer
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly INavigationService _navigationService;
        private object _currentPage;
        private Rect _backgroundRec;
        public bool isGraph { get; set; } = false;
        public ICommand NavTo { get; }
        public Rect BackgroundRec
        {
            get => _backgroundRec;
            set
            {
                _backgroundRec = value;
                OnPropertyChanged(nameof(BackgroundRec));
            }
        }
        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public MainViewModel()
        {
            _navigationService = new NavigationService(page => CurrentPage = page);
            NavTo = new RelayCommand(NavigateCommand);

        }

        private void NavigateCommand(object parameter)
        {
            string? action = parameter as string;

            if(isGraph)
            {

            }
            switch (action)
            {
                case "SelectionSort":
                    _navigationService.NavigateTo<SelectionSort>();
                    break;
                case "QuickSort":
                    _navigationService.NavigateTo<QuickSort>();
                    break;
                case "BubbleSort":
                    _navigationService.NavigateTo<BubbleSort>();
                    break;
                case "LinearSearch":
                    _navigationService.NavigateTo<LinearSearch>();
                    break;
                case "BinarySearch":
                    _navigationService.NavigateTo<BinarySearch>();
                    break;
                case "FibonacciSearch":
                    _navigationService.NavigateTo<FibonacciSearch>();
                    break;
                default:
                    throw new Exception("Page not Found!");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
