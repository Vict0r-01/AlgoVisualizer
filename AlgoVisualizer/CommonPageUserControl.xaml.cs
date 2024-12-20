﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AlgoVisualizer
{

    public partial class CommonPageUserControl : UserControl
    {
        private readonly CommonPageViewModel _viewModel;
        private readonly List<Grid> _innerGrid;
        private readonly List<byte> _innerColumnTracker;
        protected Canvas GraphCanvas { get; set; }
        public bool isGraph { get; set; }
        public CommonPageUserControl(bool isGraph = false)
        {
            InitializeComponent();
            _viewModel = new CommonPageViewModel(isGraph);
            _innerGrid = new List<Grid>();
            _innerColumnTracker = new List<byte>();

            DataContext = _viewModel;
            this.isGraph = isGraph;
            
            if (isGraph)
            {
                StyleControl.Style = Application.Current.FindResource("GraphStyle") as Style;
                AddEdge();
            }
            else StyleControl.Style = Application.Current.FindResource("ArrayStyle") as Style;
            
            _viewModel.CreateStepAction = UpdateGrid;
            _viewModel.CreateEdgesAction = AddEdge;
            _viewModel.ClearCanvasAction = ClearCanvas;
        }

        private void ClearCanvas()
        {
            OverlayCanvas.Children.Clear();
        }
        private void AddEdge()
        {
            foreach (var item in _viewModel.SampleGraph)
            {
                foreach (var edge in item.Edge)
                {
                    if (edge != null && !OverlayCanvas.Children.Contains(edge))
                    {
                        OverlayCanvas.Children.Add(edge);
                    }
                }   
            }
        }
        private void ClearInnerGrid()
        {
            _innerGrid.Clear();
            _innerColumnTracker.Clear();
        }
        private void ClearGrid()
        {
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.Children.Clear();
            ClearInnerGrid();
        }
        private void UpdateGrid()
        {
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = GridLength.Auto;
            //-------------------Clear Grid once done-------------------
            if (_viewModel.AlgoReady)
            {
                ClearGrid();
                DynamicGrid.RowDefinitions.Add(rowDef);
                DynamicGrid.Children.Add(StyleControl);
                return;
            }

            int stepHeight = _viewModel.StepHeight;
            int currentHeight = _viewModel.HeightCounter;

            //-------------------Add Row If needed-------------------
            if (currentHeight >= DynamicGrid.RowDefinitions.Count - 1)
            {
                DynamicGrid.RowDefinitions.Add(rowDef);
                _innerGrid.Add(new Grid());
                _innerColumnTracker.Add(0);
                DynamicGrid.Children.Add(_innerGrid[^1]);
                Debug.Write("Created InnerGrid\n");
                Grid.SetRow(_innerGrid[^1], stepHeight);

                TextBlock stepText = new TextBlock()
                {
                    Text = "Step " + (DynamicGrid.RowDefinitions.Count - 1) + ": ",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(5),
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Color.FromRgb(245, 233, 66))
                };
                DynamicGrid.Children.Add(stepText);
                Grid.SetRow(stepText, stepHeight);
            }

            WrapPanel wrappingPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            //-------------------Add Content to Correct Column-------------------
            var gridLength = new GridLength(1, GridUnitType.Star);

            _innerGrid[currentHeight - 1].ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = gridLength
            });
            _innerGrid[currentHeight -1].Children.Add(wrappingPanel);
            Grid.SetColumn(wrappingPanel, _innerColumnTracker[currentHeight -1]++);

            foreach (var t in _viewModel.PrevState)
            {
                Border border = new Border
                {
                    Background = t.IndexColor,
                    BorderThickness = new Thickness(1),
                    Width = 30,
                    Height = 35
                };
                wrappingPanel.Children.Add(border);
                TextBlock arrBlock = new TextBlock
                {
                    Text = t.Value.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(5),
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Color.FromRgb(245, 233, 66))
                };
                border.Child = arrBlock;
            }

        }
        //-------------------Define the ParentIdentifier dependency property-------------------
        public string ParentIdentifier
        {
            get => (string)GetValue(ParentIdentifierProperty);
            set => SetValue(ParentIdentifierProperty, value);
        }

        //-------------------Register the ParentIdentifier property-------------------
        public static readonly DependencyProperty ParentIdentifierProperty =
            DependencyProperty.Register("ParentIdentifier", typeof(string), typeof(CommonPageUserControl));

        
    }
}
