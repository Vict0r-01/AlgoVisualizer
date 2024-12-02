using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AlgoVisualizer
{
    public class CommonPageViewModel : INotifyPropertyChanged
    {
        //-------------------Private Variable-------------------
        private readonly Random _random = new();
        private GridLength _logHeight = new(250);
        private byte _selectedSpeed;
        private string _logText = "";
        private int _interval;
        private bool _algoRunning;
        private ObservableCollection<ArrayItem> _arrCopy;
        private ObservableCollection<GraphItem> _graphCopy;
        private string _contentPlayButton = "\u25B6";
        private bool _isPaused;
        private byte _stepHeight;
        private CancellationTokenSource _cancellationToken = new();
        private List<ArrayItem> _prevState;
        private int _search;
        private bool _steppingFlag = false;
        private byte[,]? _adjMatrix;
        //-------------------Public variables-------------------
        public ObservableCollection<string> Speeds { get; private set; }
        public ICommand GenericCommand { get; }
        public ICommand PlayCommand { get; }
        public ObservableCollection<ArrayItem> SampleArray { get; private set; }
        public ObservableCollection<GraphItem> SampleGraph { get; private set; }
        public Action CreateStepAction { get; set; }
        public Action CreateEdgesAction { get; set; }
        public Action ClearCanvasAction { get; set; }
        public List<ArrayItem> PrevState => _prevState;
        public bool AlgoCompleted { get; private set; }
        public bool AlgoReady { get; private set; } = true;
        public int HeightCounter { get; set; }
        public bool isGraph { get; set; }
        //-------------------Data Binding Variables-------------------

        public byte StepHeight
        {
            get => _stepHeight;
            set
            {
                _stepHeight = value;
                OnPropertyChanged(nameof(StepHeight));
            }
        }
        public string ContentPlayButton
        {
            get => _contentPlayButton;
            set
            {
                _contentPlayButton = value;
                OnPropertyChanged(nameof(ContentPlayButton));
            }
        }
        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                OnPropertyChanged(nameof(LogText));
            }
        }
        public byte SelectedSpeed
        {
            get => _selectedSpeed;
            set
            {
                if (_selectedSpeed == value) return;
                _selectedSpeed = value;
                OnPropertyChanged(nameof(SelectedSpeed));

                OnSpeedSelected();

            }
        }
        public GridLength LogHeight
        {
            get => _logHeight;
            set
            {
                _logHeight = value;
                OnPropertyChanged(nameof(LogHeight));
            }
        }

        //-------------------Constructor-------------------
        public CommonPageViewModel(bool isGraph)
        {
            this.isGraph = isGraph;
            GenericCommand = new RelayCommand(ExecuteCommand);
            PlayCommand = new RelayCommand(PlayAlgoCommand);
            if (isGraph)
            {
                SampleGraph = new ObservableCollection<GraphItem>();
                _graphCopy = new ObservableCollection<GraphItem>();
            }
            else
            {
                SampleArray = new ObservableCollection<ArrayItem>();
                _arrCopy = new ObservableCollection<ArrayItem>();
            }
            CreateSample();
            _stepHeight = 0;
            Speeds = ["0.5x", "1x", "1.5x", "2.0x", "5.0x"];
            SelectedSpeed = 1;
            _interval = 750;
        }

        //-------------------Start Algo when Play Button Clicked-------------------
        private void PlayAlgoCommand(object parameter)
        {
            if (AlgoReady)
            {
                _algoRunning = true;
                AlgoReady = false;
                LogText += "------Starting:------";
                ContentPlayButton = "\u23F8";

                string? algorithm = parameter as string;

                if (algorithm != null) StartAlgorithm(algorithm);
            } else PlayButtonState();
        }
        //-------------------Create Sample Array-------------------
        private void CreateSample()
        {
            CreateItems();
            if(isGraph)
                CreateAdjMatrix();
        }
        private void CreateItems()
        {
            int min = 0;
            int max = 30;

            for (var i = 0; i < _random.Next(6, 20); i++)
            {
                int num = _random.Next(99);
                
                if (isGraph)
                {
                    int x = _random.Next(min, max);
                    int y = _random.Next(250);
                    SampleGraph.Add(new GraphItem { Value = i, X = x, Y = y });
                    _graphCopy.Add(new GraphItem { Value = i, X = x, Y = y });
                    min = (max + 35);
                    max = (min + 30);
                }
                else
                {
                    SampleArray.Add(new ArrayItem { Value = num });
                    _arrCopy.Add(new ArrayItem { Value = num });
                }
            }
        }

        private void CreateAdjMatrix()
        {
            var count = SampleGraph.Count;
            _adjMatrix = new byte[count, count];
            for(int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++) {
                    if (_random.Next(4) == 0)
                    {
                        byte weight = (byte)_random.Next(20);
                        _adjMatrix[i, j] = weight;
                        _adjMatrix[j, i] = weight;
                        CreateEdges(i,j);
                    }
                }
            }
            CreateEdgesAction?.Invoke();
            for(int x = 0; x < count; x++)
            {
                for(int y = 0; y < count; y++)
                {
                    Debug.Write(" " + _adjMatrix[x, y]);
                }
                Debug.Write("\n");
            }
        }

        private void CreateEdges(int row, int col)
        {
            int offset = 15;
            var line = new Line
            {
                X1 = SampleGraph[row].X + offset,
                Y1 = SampleGraph[row].Y + offset,
                X2 = SampleGraph[col].X + offset,
                Y2 = SampleGraph[col].Y + offset,
                StrokeThickness = 2
            };

            // Create the multibinding
            var multiBinding = new MultiBinding
            {
                Converter = new MultiColorConverter(),
                Mode = BindingMode.OneWay
            };

            // Bind to IndexColor of both vertices
            multiBinding.Bindings.Add(new Binding("IndexColor") { Source = SampleGraph[row], Mode = BindingMode.OneWay });
            multiBinding.Bindings.Add(new Binding("IndexColor") { Source = SampleGraph[col], Mode = BindingMode.OneWay });

            // Apply the MultiBinding to the Stroke property
            BindingOperations.SetBinding(line, Line.StrokeProperty, multiBinding);

            SampleGraph[row].Edge.Add(line);
            _graphCopy[row].Edge.Add(line);
            SampleGraph[col].Edge.Add(SampleGraph[row].Edge[^1]);
            _graphCopy[col].Edge.Add(_graphCopy[row].Edge[^1]);

        }
        //-------------------Change Speed When Changed-------------------
        private void OnSpeedSelected()
        {
            _interval = SelectedSpeed switch
            {
                0 => 1125,
                1 => 750,
                2 => 560,
                3 => 375,
                4 => 150,
                _ => throw new Exception("Speed Not Found!")
            };
        }

        //-------------------Generic Command Controls-------------------
        private void ExecuteCommand(object parameter)
        {
            string? action = parameter as string;

            switch (action)
            {
                case "Refresh":
                    if (isGraph)
                    {
                        SampleGraph.Clear();
                        _graphCopy.Clear();
                        _adjMatrix = null;
                        ClearCanvasAction?.Invoke();
                    }
                    else
                    {
                        SampleArray.Clear();
                        _arrCopy.Clear();
                    }                  
                    _cancellationToken.Cancel();
                    CreateSample();
                    Restart();
                    break;
                case "Log":
                    ChangeLogSize();
                    break;
            }
        }

        //-------------------Restart Algorithm-------------------
        private void Restart()
        {
            _algoRunning = false;
            AlgoCompleted = false;
            AlgoReady = true;
            _isPaused = false;
            ContentPlayButton = "▶";
            HeightCounter = 0;
            StepHeight = 0;
            if(_steppingFlag) CreateStepAction?.Invoke();
        }

        //-------------------Keep track of play button state: Paused/Running/Completed-------------------
        private void PlayButtonState()
        {
            if (_algoRunning)
            {
                ContentPlayButton = "▶";
                _algoRunning = false;
                TogglePause();
            }
            else if (AlgoCompleted)
            {
                if (isGraph)
                {
                    SampleGraph = new ObservableCollection<GraphItem>(_graphCopy.Select(item => item.Clone()));
                    OnPropertyChanged(nameof(SampleGraph));
                }
                else
                {
                    SampleArray = new ObservableCollection<ArrayItem>(_arrCopy.Select(item => item.Clone()));
                    OnPropertyChanged(nameof(SampleArray));
                }
                
                Restart();
            }
            else
            {
                ContentPlayButton = "⏸";
                TogglePause();
                _algoRunning = true;
            }
        }

        //-------------------Pause the Algorithm-------------------
        private void TogglePause()
        {
            _isPaused = !_isPaused;
        }

        //-------------------Minimize Log Size-------------------
        private void ChangeLogSize()
        {
            GridLengthConverter logHeight = new GridLengthConverter();
            String gridHeight = logHeight.ConvertToString(LogHeight) ?? string.Empty;
            if (gridHeight != string.Empty)
                LogHeight = gridHeight != "0" ? new GridLength(0) : new GridLength(250);
        }

        private enum IndexColor
        {
            Green,
            Blue,
            Red
        }
        //-------------------Highlight Array Index-------------------
        private void ToggleHighlightIndex(IItem index, bool toggle = false, IndexColor color = IndexColor.Green)
        {
            if(!toggle)
            {
                index.IndexColor = Brushes.Black;
                return;
            }
            Brush brushColor = color switch
            {
                IndexColor.Green => new SolidColorBrush(Color.FromRgb(25, 48, 45)),
                IndexColor.Blue => new SolidColorBrush(Color.FromRgb(15, 26, 150)),
                IndexColor.Red => new SolidColorBrush(Color.FromRgb(166, 26, 10)),
                _ => throw new NotImplementedException()
            };
            index.IndexColor = brushColor;
        }

        //-------------------Create Step/Branch when needed to display-------------------
        private void CreateStep(int start, int end)
        {
            _steppingFlag = true;
            _prevState = SampleArray.Skip(start).Take(end + 1 - start).ToList();
            HeightCounter++;
            CreateStepAction?.Invoke();
            if(HeightCounter >= StepHeight)
                StepHeight++;
        }

        public event PropertyChangedEventHandler PropertyChanged;
            
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //-----------------------------Algorithms--------------------------------------------------------------------------------------------------------


        //-------------------Call Algorithm Functions-------------------
        private async void StartAlgorithm(string algorithm)
        {
            _cancellationToken = new CancellationTokenSource();
            int searching = 0;
            if (!isGraph)
            {
                _search = _random.Next(0, SampleArray.Count - 1);
                searching = SampleArray[_search].Value;
            }

            try
            {
                switch (algorithm)
                {
                    case "QuickSort":
                        await QuickSort(0, SampleArray.Count - 1);
                        break;
                    case "SelectionSort":
                        await SelectionSort();
                        break;
                    case "BubbleSort":
                        await BubbleSort();
                        break;
                    case "LinearSearch":
                        await LinearSearch(searching);
                        break;
                    case "BinarySearch":
                        await BinarySearch(searching);
                        break;
                    case "FibonacciSearch":
                        await FibonacciSearch(searching);
                        break;
                    case "Dijkstra":
                        await dijkstra(_random.Next(0, SampleGraph.Count - 1));
                        break;
                    case "Kruskal":
                        await kruskal();
                        break;
                    default: throw new Exception("Algorithm Not Found!");
                }
            }
            catch (OperationCanceledException)
            {
                LogText += "\n------Algorithm Stopped!------\n";
                return;
            }

            AlgoReady = false;
        }

        //-------------------Display End of Algorithm-------------------
        private void AlgorithmEnd()
        {
            LogText += "\n------Algorithm Completed!------\n";
            _algoRunning = false;
            ContentPlayButton = "⟳";
            AlgoCompleted = true;
        }

        private void SortArray()
        {
            var sortedItems = SampleArray.OrderBy(item => item.Value).ToList();

            // Clear and refill SampleArray in sorted order
            SampleArray.Clear();
            foreach (var item in sortedItems)
            {
                SampleArray.Add(item);
            }
        }
        //-------------------Check if Algorithm is paused-------------------
        private async Task CheckPause()
        {
            while (_isPaused) await Task.Delay(100, _cancellationToken.Token);
        }

        //-------------------Delay based on speed-------------------
        private async Task DelayTask()
        {
            await CheckPause();
            await Task.Delay(_interval, _cancellationToken.Token);
        }

        //-------------------Selection Sort Algorithm-------------------
        private async Task SelectionSort()
        {
            for (int i = 0; i < SampleArray.Count; i++)
            {
                int min = i;
                
                for (int j = i + 1; j < SampleArray.Count; j++)
                {
                    if (SampleArray[j].Value < SampleArray[min].Value)
                    {
                        min = j;
                        LogText += "\nMin: " + SampleArray[min].Value;
                        ToggleHighlightIndex(SampleArray[min], true);
                        await DelayTask();
                        ToggleHighlightIndex(SampleArray[min]);
                    }
                }

                if (min != i)
                {
                    ToggleHighlightIndex(SampleArray[min], true);
                    ToggleHighlightIndex(SampleArray[i], true);
                    LogText += "\nSwitching " + SampleArray[min].Value + " with " + SampleArray[i].Value;
                    await DelayTask();
                    (SampleArray[min], SampleArray[i]) = (SampleArray[i], SampleArray[min]);
                    await DelayTask();
                    ToggleHighlightIndex(SampleArray[min]);
                    ToggleHighlightIndex(SampleArray[i]);
                }
            }
            AlgorithmEnd();
        }

        //-------------------Quick Sort Algorithm-------------------
        private async Task QuickSort(int start, int end)
        {
            if (start >= end) return;
            int partition = await Partition();
            await QuickSort(start, partition-1);
            await QuickSort(partition + 1, end);
            HeightCounter--;
            if(start == 0 && end == SampleArray.Count-1) AlgorithmEnd();
            return;

            //-------------------Inner Function for Partition-------------------
            async Task<int> Partition()
            {
                ToggleHighlightIndex(SampleArray[end], true);
                int pivot = SampleArray[end].Value;
                int i = start-1;
                for (int j = start; j <= end-1; j++)
                {
                    if (SampleArray[j].Value >= pivot || ++i == j) continue;
                    ToggleHighlightIndex(SampleArray[j], true);
                    ToggleHighlightIndex(SampleArray[i], true);
                    LogText += "\nSwitching " + SampleArray[i].Value + " with " + SampleArray[j].Value;
                    await DelayTask();
                    (SampleArray[i], SampleArray[j]) = (SampleArray[j], SampleArray[i]);
                    await DelayTask();
                    ToggleHighlightIndex(SampleArray[j]);
                    ToggleHighlightIndex(SampleArray[i]);
                }
                var pivotPos = i + 1;
                ToggleHighlightIndex(SampleArray[pivotPos], true);
                LogText += "\nSwitching pivot with " + SampleArray[pivotPos].Value;
                await DelayTask();
                (SampleArray[pivotPos], SampleArray[end]) = (SampleArray[end], SampleArray[pivotPos]);
                await DelayTask();
                if(end != pivotPos) ToggleHighlightIndex(SampleArray[end]);
                CreateStep(start, end);
                ToggleHighlightIndex(SampleArray[pivotPos]);
                return pivotPos;
            }
        }

        //-------------------Bubble Sort Algorithm-------------------
        private async Task BubbleSort()
        {
            int len = SampleArray.Count;
            bool flag = true;
            for (int i = 0; (i <= len - 2) && flag; i++)
            {
                flag = false;
                for (int j = 0; j <= len - 2 - i; j++)
                {
                    if (SampleArray[j].Value > SampleArray[j + 1].Value)
                    {
                        flag = true;
                        ToggleHighlightIndex(SampleArray[j], true);
                        ToggleHighlightIndex(SampleArray[j + 1], true);
                        LogText += "\nSwapping " + SampleArray[j + 1].Value + " and " + SampleArray[j].Value;
                        await DelayTask();
                        (SampleArray[j + 1], SampleArray[j]) = (SampleArray[j], SampleArray[j + 1]);
                        await DelayTask();
                        ToggleHighlightIndex(SampleArray[j]);
                        ToggleHighlightIndex(SampleArray[j + 1]);
                    }
                }
                ToggleHighlightIndex(SampleArray[len - 2 - i], true);
                CreateStep(0, SampleArray.Count - 1);
                ToggleHighlightIndex(SampleArray[len - 2 - i]);
            }
            AlgorithmEnd();
        }

        //-------------------Linear Search Algorithm-------------------
        private async Task LinearSearch(int n)
        {
            LogText += "\nSearching number: " + n + "...";
            ToggleHighlightIndex(SampleArray[_search], true, IndexColor.Blue);
            for (int i = 0; i < SampleArray.Count; i++)
            {
                ToggleHighlightIndex(SampleArray[i], true);
                await DelayTask();
                if (SampleArray[i].Value == n)
                {
                    LogText += "\n" + n + " found at index: " + i;
                    AlgorithmEnd();
                    return;
                }
                ToggleHighlightIndex(SampleArray[i], true, IndexColor.Red);
                await DelayTask();
                ToggleHighlightIndex(SampleArray[i]);
            }
            LogText += "\nValue Not Found!";
            AlgorithmEnd();
        }

        //-------------------Binary Search Algorithm-------------------
        private async Task BinarySearch(int n)
        {
            int min = 0;
            int max = SampleArray.Count - 1;
            SampleArray[_search].IndexColor = new SolidColorBrush(Color.FromRgb(15, 26, 150));
            LogText += "\nSorting Array...";
            await DelayTask();
            SortArray();
            await DelayTask();
            LogText += "\nSearching number: " + n + "...";
            while (min <= max)
            {
                int mid = (min + max) / 2;                
                ToggleHighlightIndex(SampleArray[mid], true);
                await DelayTask();
                if (n == SampleArray[mid].Value)
                {
                    LogText += "\n" + n + " found at index: " + mid;
                    AlgorithmEnd();
                    return;
                }
                else if (n < SampleArray[mid].Value)
                {
                    LogText += "\n" + n + " is less than " + mid;
                    max = mid - 1;
                }
                else
                {
                    LogText += "\n" + n + " is more than " + mid;
                    min = mid + 1;
                }
                ToggleHighlightIndex(SampleArray[mid], true, IndexColor.Red);
                await DelayTask();
                ToggleHighlightIndex(SampleArray[mid]);
                CreateStep(min, max);
            }
            LogText += "\nValue Not Found!";
            AlgorithmEnd();
        }

        //-------------------Fibonacci Search Algorithm-------------------
        private async Task FibonacciSearch(int n)
        {
            LogText += "\nSorting Array...";
            ArrayItem search = SampleArray[_search];
            await DelayTask();
            SortArray();
            await DelayTask();
            LogText += "\nSearching Number: " + n + "...";
            ToggleHighlightIndex(search, true, IndexColor.Blue);
            int l = SampleArray.Count;

            // Initialize Fibonacci numbers
            int fib1 = 0, fib2 = 1, fib3 = fib1 + fib2;

            // Find the smallest Fibonacci number greater than or equal to l
            LogText += "\nFinding smaller Fibonacci number greater than or equal to " + l;
            await DelayTask();
            while (fib3 < l)
            {
                fib1 = fib2;
                fib2 = fib3;
                fib3 = fib1 + fib2;
            }
            LogText += "\nFib 1: " + fib1 + "\nFib 2: " + fib2 + "\nFib 3: " + fib3;
            // Initialize variables for the current and previous split points
            int offset = -1;
            while (fib3 > 1)
            {
                int i = Math.Min(offset + fib2, l - 1);
                ToggleHighlightIndex(SampleArray[i], true);
                await DelayTask();
                // If x is greater than the value at index i, move the split point to the right
                if (SampleArray[i].Value < n)
                {
                    fib3 = fib2;
                    fib2 = fib1;
                    fib1 = fib3 - fib2;
                    offset = i;
                }

                // If x is less than the value at index i, move the split point to the left
                else if (SampleArray[i].Value > n)
                {
                    fib3 = fib1;
                    fib2 = fib2 - fib1;
                    fib1 = fib3 - fib2;
                }

                // If x is equal to the value at index i, return the index
                else
                {
                    LogText += "\n" + n + " found at index: " + i;
                    AlgorithmEnd();
                    return;
                }
                ToggleHighlightIndex(SampleArray[i]);
                ToggleHighlightIndex(SampleArray[fib1], true, IndexColor.Red);
                ToggleHighlightIndex(SampleArray[fib2], true, IndexColor.Red);
                ToggleHighlightIndex(SampleArray[fib3], true, IndexColor.Red);
                await DelayTask();
                ToggleHighlightIndex(SampleArray[fib1]);
                ToggleHighlightIndex(SampleArray[fib2]);
                ToggleHighlightIndex(SampleArray[fib3]);
                LogText += "\nFib 1: " + fib1 + "\nFib 2: " + fib2 + "\nFib 3: " + fib3 + "\n----------------------";
            }

            // If x is not found in the array, return -1
            if (fib2 == 1 && SampleArray[offset + 1].Value == n)
            {
                LogText += "\n" + n + " found at index: " + (offset + 1);
            }
            else
            {
                LogText += "\nValue Not Found!";
            }
            AlgorithmEnd();
        }

        private async Task<int> minDistance(int[] dist,
                    bool[] sptSet)
        {
            // Initialize min value
            int min = int.MaxValue, min_index = -1;
            int V = _adjMatrix.GetLength(0);
            for (int v = 0; v < V; v++)
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }

            return min_index;
        }

        //-------------------Dijkstra Algorithm-------------------
        private async Task dijkstra(int src)
        {
            int V = _adjMatrix.GetLength(0);
            LogText += "\nStarting from: " + src;
            //Array to hold shortest distance
            int[] dist = new int[V];

            //True if shortest distance found
            bool[] distFinalized = new bool[V];

            // Initialize all distances as
            // INFINITE and distFinalized[] as false
            for (int i = 0; i < V; i++)
            {
                dist[i] = int.MaxValue;
                distFinalized[i] = false;
            }

            // Distance of source vertex is 0
            dist[src] = 0;

            // Find shortest path for all vertices
            for (int count = 0; count < V; count++)
            {
                
                await DelayTask();
                //Find min distance
                int u = await minDistance(dist, distFinalized);
                ToggleHighlightIndex(SampleGraph[u], true, IndexColor.Green);
                await DelayTask();
                // Mark the picked vertex as processed
                distFinalized[u] = true;
                LogText += "\n---Vertex " + u + " Processed---";

                // Update dist value of the adjacent
                // vertices of the picked vertex.
                for (int v = 0; v < V; v++)
                {
                    
                    // Update dist[v] only if is not in
                    // distFinalized, there is an edge from u
                    // to v, and total weight of path
                    // from src to v through u is smaller
                    // than current value of dist[v]
                    if (!distFinalized[v] && _adjMatrix[u, v] != 0 &&
                         dist[u] != int.MaxValue && dist[u] + _adjMatrix[u, v] < dist[v])
                    {
                        ToggleHighlightIndex(SampleGraph[v], true, IndexColor.Red);
                        await DelayTask();
                        dist[v] = dist[u] + _adjMatrix[u, v];
                        LogText += "\nV(" + v + ") distance from V(" + src + "): " + dist[v];
                        ToggleHighlightIndex(SampleGraph[v]);
                    }
                    
                }

            }
            foreach(var item in distFinalized) Debug.Write(item + " ");
            AlgorithmEnd();
        }


        // Finds MST using Kruskal's algorithm

        private struct Edge : IComparable<Edge>
        {
            public int Source { get; set; }
            public int Destination { get; set; }
            public int Weight { get; set; }

            public int CompareTo(Edge other)
            {
                return Weight.CompareTo(other.Weight);
            }
        }
        private async Task kruskal()
        {
            int mincost = 0; // Cost of min MST.
            int V = _adjMatrix.GetLength(0);
            int[] parent = new int[V];
            int[] rank = new int[V];
            List<Edge> edges = new List<Edge>();
            AddSortedEdges();
            // Initialize sets of disjoint sets.
            for (int i = 0; i < V; i++)
            {
                parent[i] = i;
                rank[i] = 0;
            }

            // Include minimum weight edges one by one
            int edge_count = 0;
            List<int> exclude = new();
            foreach (var edge in edges)
            {
                if (edge_count == V - 1)
                    break;

                int x = Find(edge.Source);
                int y = Find(edge.Destination);

                if (x != y) // If including this edge does not form a cycle
                {
                    await DelayTask();
                    ToggleHighlightIndex(SampleGraph[edge.Source], true);
                    ToggleHighlightIndex(SampleGraph[edge.Destination], true);
                    LogText += string.Format("\nEdge {0}: ({1}, {2}) cost: {3}",
                                                edge_count++, edge.Source, edge.Destination, edge.Weight);
                    mincost += edge.Weight;
                    Union(x, y);
                }
                else
                {
                    exclude.Add(edge.Source);
                    exclude.Add(edge.Destination);
                }
                await DelayTask();
            }
            
            LogText += "\nMinimum cost= " + mincost;
            LogText += "\nEdges to exclude:";
            for(int x = 0; x < exclude.Count - 1; x += 2) {

                foreach (var edge in SampleGraph[exclude[x]].Edge) 
                {
                    if (edge.X2 == (SampleGraph[exclude[x + 1]].X + 15)) edge.Stroke = Brushes.Red;
                }
                LogText += " (" + exclude[x] + "," + exclude[x + 1] + ")";
            }
            if (exclude.Count < 1) LogText += " None";
            AlgorithmEnd();


            //------------------Inner Methods-------------------
            void Union(int i, int j)
            {
                int xRoot = Find(i);
                int yRoot = Find(j);

                if (rank[xRoot] < rank[yRoot])
                    parent[xRoot] = yRoot;
                else if (rank[xRoot] > rank[yRoot])
                    parent[yRoot] = xRoot;
                else
                {
                    parent[yRoot] = xRoot;
                    rank[xRoot]++;
                }
            }

            int Find(int i)
            {
                if (parent[i] != i)
                {
                    parent[i] = Find(parent[i]);
                }
                return parent[i];

                
            }

            void AddSortedEdges()
            {
                for (int i = 0; i < V; i++)
                {
                    for (int j = i + 1; j < V; j++)
                    {
                        if (_adjMatrix[i,j] != 0) edges.Add(new Edge { Source = i, Destination = j, Weight = _adjMatrix[i,j] });
                    }
                }
                edges.Sort();
            }
        }
        
    }
}
