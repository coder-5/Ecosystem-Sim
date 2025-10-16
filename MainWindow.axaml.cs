using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcosystemSim
{
    public partial class MainWindow : Window
    {
        public CancellationTokenSource appCancellation = new CancellationTokenSource();
        Random rand = new Random();
        public Ecosystem ecosystem = new Ecosystem();
        bool paused = false;
        bool simulationLineGraphsvisible = true;
        bool simulationProgressBarVisible = true;
        int max_simulation_steps = 10000;
        progressBar progress;
        public bool running;
        public MainWindow()
        {
            ecosystem.start();

            InitializeComponent();

            this.Closing += (s, e) =>
            {
                appCancellation.Cancel();
            };

            this.KeyDown += OnKeyDown;

            for (int i = 0; i < 10; i++)
            {
                ecosystem.activeSpecies.Add(new Species("5:500:1:100:25", "5:500:0:100:25", rand.Next(0, 800), rand.Next(0, 450)));
                ecosystem.activeSpecies[i].inherit_genes();
            }
            for (int i = 0; i < 51; i++)
            {
                ecosystem.activeFood.Add(new FoodSpecies(1, rand.Next(0, 800), rand.Next(0, 450), rand.Next(1, 4), 50, 500 + rand.Next(-50, 51), 1000 + rand.Next(-50, 51)));
            }
            for (int i = 0; i < 100; i++)
            {
                ecosystem.activeWater.Add(new WaterZone(1, 650 + rand.Next(-25, 25), rand.Next(0, 450)));
            }
            for (int i = 0; i < 100; i++)
            {
                ecosystem.activeWater.Add(new WaterZone(1, 250 + rand.Next(-25, 25), rand.Next(0, 450)));
            }

            EcosystemCanvas.EcosystemData = ecosystem;

            progress = new progressBar("Finished Progress");
            progress.Show();
            if (simulationProgressBarVisible)
            {
                progress.drawProgressBar(ecosystem.simulationSteps, max_simulation_steps);
            }
        }
        private void OnKeyDown(object? sende, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                paused = !paused;
            }
        }
        public void updateSimulationSteps(int negative)
        {
            max_simulation_steps += 100 * negative;
            progress.drawProgressBar(ecosystem.simulationSteps, max_simulation_steps);
        }
        public void RunLoopCaller()
        {
            RunLoop(appCancellation.Token, progress);
        }

        private async void RunLoop(CancellationToken token, progressBar progress)
        {
            running = true;
            var populationLineGraph = new LineGraphWindow("Populations Graph");
            populationLineGraph.Show();
            var femaleToMale = new LineGraphWindow("Female v Male Line Graph");
            femaleToMale.Show();
            var sproutedToUnsprouted = new LineGraphWindow("Sprouted v Un-Sprouted Line Graph");
            sproutedToUnsprouted.Show();
            var traits = new LineGraphWindow("Traits Line Graph");
            traits.Show();
            updateGraphs(populationLineGraph, femaleToMale, sproutedToUnsprouted, traits, token, progress);
            while (!token.IsCancellationRequested && ecosystem.simulationSteps < max_simulation_steps && ecosystem.activeSpecies.Count != 0)
            {
                if (!paused)
                {
                    ecosystem.update();
                    EcosystemCanvas.Refresh();
                    await Task.Delay(10);
                }
            }
        }
        private async void updateGraphs(LineGraphWindow populationLineGraph, LineGraphWindow femaleToMale, LineGraphWindow sproutedToUnsprouted, LineGraphWindow traits, CancellationToken token, progressBar progress)
        {
            while (!token.IsCancellationRequested && simulationLineGraphsvisible && ecosystem.simulationSteps < max_simulation_steps && ecosystem.activeSpecies.Count != 0)
            {
                List<IBrush> colors = [Brushes.Red, Brushes.Green];
                populationLineGraph.drawLineGraph(new List<List<double>> { ecosystem.populationSizes, ecosystem.foodSizes }, colors, new List<string> { "Population Size", "Food Population" });
                List<IBrush> colors2 = [Brushes.Red, Brushes.Black];
                femaleToMale.drawLineGraph(new List<List<double>> { ecosystem.femaleSpecies, ecosystem.maleSpecies }, colors2, new List<string> { "Female", "Male" });
                List<IBrush> colors3 = [Brushes.Green, Brushes.Brown];
                sproutedToUnsprouted.drawLineGraph(new List<List<double>> { ecosystem.sproutedPlants, ecosystem.unSproutedPlants }, colors3, new List<string> { "Sprouted", "UnSprouted" });
                List<IBrush> colors4 = [Brushes.Green, Brushes.Blue, Brushes.Red];
                List<double> averageEyeSightSmaller = new();
                foreach (double sight in ecosystem.averageEyeSight)
                {
                    averageEyeSightSmaller.Add(sight / 10);
                }
                traits.drawLineGraph(new List<List<double>> { averageEyeSightSmaller, ecosystem.averageReproductionAge, ecosystem.averageSpeedPrey }, colors4, new List<string> { "Eye Sight", "Reproduction", "Speed" });
                if (simulationProgressBarVisible)
                {
                    progress.update(ecosystem.simulationSteps, max_simulation_steps);
                }
                await Task.Delay(100);
            }
        }
        public partial class progressBar : Window
        {
            public Canvas GraphCanvas;
            public Rectangle progressRect = new();
            public Rectangle progressRectUnfilled = new();
            public TextBlock stepsText = new();
            public Button startButton;
            public progressBar(string name)
            {
                Width = 500;
                Height = 125;
                Title = name;

                GraphCanvas = new Canvas { Background = Brushes.White };
                Content = GraphCanvas;
            }
            public void update(int amount, int goal)
            {
                if (progressRect != null)
                {
                    progressRect.Width = 490 * ((double)amount / goal);
                    progressRect.InvalidateMeasure();
                }
                if (progressRectUnfilled != null)
                {
                    progressRectUnfilled.Width = 490 - (490 * ((double)amount / goal));
                    Canvas.SetLeft(progressRectUnfilled, 5 + (490 * ((double)amount / goal)));
                    progressRectUnfilled.InvalidateMeasure();
                }
                if (stepsText != null)
                {
                    var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

                    if (mainWindow != null)
                    {
                        stepsText.Text = mainWindow.ecosystem.simulationSteps.ToString() + "/" + mainWindow.max_simulation_steps.ToString();
                    }
                }

                var mainWindw = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

                if (mainWindw != null && startButton != null)
                {
                    if (startButton.Content.ToString() != "UnPause Simulation" && mainWindw.running && mainWindw.paused)
                    {
                        startButton.Content = "UnPause Simulation";
                    }
                    else if (startButton.Content.ToString() != "Pause Simulation" && mainWindw.running && !mainWindw.paused)
                    {
                        startButton.Content = "Pause Simulation";
                    }
                }
                
                GraphCanvas.InvalidateArrange();
                GraphCanvas.InvalidateVisual(); // fix bug were ui rects arent changing size
            }
            public void drawProgressBar(int amount, int goal)
            {
                GraphCanvas.Children.Clear();

                progressRect = new Rectangle()
                {
                    Width = 490 * ((double)amount / goal),
                    Height = 50,
                    Fill = Brushes.Green
                };

                Canvas.SetLeft(progressRect, 5);
                Canvas.SetBottom(progressRect, 5);

                GraphCanvas.Children.Add(progressRect);

                progressRectUnfilled = new Rectangle()
                {
                    Width = 490 - (490 * (amount / goal)),
                    Height = 50,
                    Fill = Brushes.Blue
                };
                
                Canvas.SetLeft(progressRectUnfilled, 5 + (490 * ((double)amount / goal)));
                Canvas.SetBottom(progressRectUnfilled, 5);

                GraphCanvas.Children.Add(progressRectUnfilled);

                startButton = new Button()
                {
                    Width = 490,
                    Height = 30,
                    Content = "Start Simulation"
                };

                Canvas.SetLeft(startButton, 5);
                Canvas.SetBottom(startButton, 5 + 50 + 5);

                var mainWindw = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;
                if (mainWindw != null)
                {
                    if (mainWindw.running && !mainWindw.paused)
                    {
                        startButton.Content = "Pause Simulation";
                    }
                    else if (mainWindw.running && mainWindw.paused)
                    {
                        startButton.Content = "UnPause Simulation";
                    }
                }

                startButton.Click += (s, e) =>
                {
                    var mainWindw = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;
                    if (mainWindw != null)
                    {
                        if (mainWindw.running)
                        {
                            mainWindw.paused = !mainWindw.paused;  // Crashing happens here
                        }
                        else
                        {
                            mainWindw.RunLoopCaller();
                        }
                    }
                };

                GraphCanvas.Children.Add(startButton);

                var startButton1 = new Button()
                {
                    Width = 100,
                    Height = 30,
                    Content = "-100 Step"
                };

                Canvas.SetLeft(startButton1, 5);
                Canvas.SetBottom(startButton1, 5 + 50 + 5 + 30 + 5);

                startButton1.Click += (s, e) =>
                {
                    var mainWindw = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;
                    if (mainWindw != null)
                    {
                        mainWindw.updateSimulationSteps(-1);
                    }
                };

                GraphCanvas.Children.Add(startButton1);

                var startButton2 = new Button()
                {
                    Width = 100,
                    Height = 30,
                    Content = "+100 Step"
                };

                Canvas.SetLeft(startButton2, 5 + 100 + 5 + 200 + 5);
                Canvas.SetBottom(startButton2, 5 + 50 + 5 + 30 + 5);

                startButton2.Click += (s, e) =>
                {
                    var mainWindw = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;
                    if (mainWindw != null)
                    {
                        mainWindw.updateSimulationSteps(1);
                    }
                };

                GraphCanvas.Children.Add(startButton2);

                var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

                stepsText = new TextBlock()
                {
                    Width = 200,
                    Height = 30,
                    TextAlignment = TextAlignment.Center,
                    Text = "Main Window Not Found"
                };

                Canvas.SetLeft(stepsText, 5 + 100 + 5);
                Canvas.SetBottom(stepsText, 5 + 50 + 5 + 30 + 5);

                if (mainWindow != null)
                {
                    stepsText.Text = mainWindow.max_simulation_steps.ToString();
                }

                GraphCanvas.Children.Add(stepsText);
            }
        }
        public partial class LineGraphWindow : Window
        {
            public Canvas GraphCanvas;
            public LineGraphWindow(string name)
            {
                Width = 500;
                Height = 300;
                Title = name;

                GraphCanvas = new Canvas { Background = Brushes.White };
                Content = GraphCanvas;
            }

            public void drawLineGraph(List<List<double>> datas, List<IBrush> colors, List<string> names)
            {
                GraphCanvas.Children.Clear();

                double yMax = datas.SelectMany(d => d).DefaultIfEmpty(1).Max();
                yMax *= 1.1;

                for (int j = 0; j < datas.Count; j++)
                {
                    List<double> data = datas[j];
                    var color = colors[j];

                    double width = GraphCanvas.Bounds.Width;
                    double height = GraphCanvas.Bounds.Height;
                    double xStep = width / (data.Count - 1);
                    double yScale = height / yMax;

                    var Polyline = new Polyline
                    {
                        Stroke = color,
                        StrokeThickness = 2
                    };

                    for (int i = 0; i < data.Count; i++)
                    {
                        double x = i * xStep;
                        double y = height - (data[i] * yScale);
                        Polyline.Points.Add(new Avalonia.Point(x, y));
                    }

                    GraphCanvas.Children.Add(Polyline);
                }

                double legendX = 10;
                double legendY = 10;
                double legendSpacing = 20;

                for (int h = 0; h < datas.Count; h++)
                {
                    var rect = new Rectangle()
                    {
                        Width = 15,
                        Height = 15,
                        Fill = colors[h]
                    };
                    Canvas.SetLeft(rect, legendX);
                    Canvas.SetTop(rect, legendY + h * legendSpacing);
                    GraphCanvas.Children.Add(rect);

                    string labelText = names[h];
                    var text = new TextBlock
                    {
                        Text = labelText,
                        Foreground = Brushes.Black,
                        FontSize = 14
                    };
                    Canvas.SetLeft(text, legendX + 20);
                    Canvas.SetTop(text, legendY + h * legendSpacing - 2);
                    GraphCanvas.Children.Add(text);
                }
            }
        }
    }
}