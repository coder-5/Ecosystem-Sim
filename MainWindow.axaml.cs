using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace EcosystemSim
{
    public partial class MainWindow : Window
    {
        Random rand = new Random();
        Ecosystem ecosystem = new Ecosystem();
        bool paused = false;
        public MainWindow()
        {
            ecosystem.start();

            InitializeComponent();

            this.KeyDown += OnKeyDown;

            for (int i = 0; i < 10; i++)
            {
                ecosystem.activeSpecies.Add(new Species("5:500:1:100:25", "5:500:0:100:25", rand.Next(0, 800), rand.Next(0, 450)));
                ecosystem.activeSpecies[i].inherit_genes();
            }
            for (int i = 0; i < 26; i++)
            {
                ecosystem.activeFood.Add(new FoodSpecies(1, rand.Next(0, 800), rand.Next(0, 450), rand.Next(1, 4)));
            }
            for (int i = 0; i < 200; i++)
            {
                ecosystem.activeWater.Add(new WaterZone(1, 400 + rand.Next(-25, 25), rand.Next(0, 450)));
            }

            EcosystemCanvas.EcosystemData = ecosystem;

            RunLoop();
        }
        private void OnKeyDown(object? sende, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                paused = !paused;
            }
        }

        private async void RunLoop()
        {
            var populationLineGraph = new LineGraphWindow();
            populationLineGraph.Show();
            while (true)
            {
                if (!paused)
                {
                    ecosystem.update();
                    EcosystemCanvas.Refresh();
                    List<IBrush> colors = [Brushes.Red, Brushes.Green];
                    populationLineGraph.drawLineGraph(new List<List<double>> { ecosystem.populationSizes, ecosystem.foodSizes }, colors);
                }
                await Task.Delay(100);
            }
        }
    }
    public partial class LineGraphWindow : Window
    {
        public Canvas GraphCanvas;
        public LineGraphWindow()
        {
            Width = 500;
            Height = 300;
            Title = "line Graph";

            GraphCanvas = new Canvas { Background = Brushes.White };
            Content = GraphCanvas;
        }

        public void drawLineGraph(List<List<double>> datas, List<IBrush> colors)
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
        }
    }
}