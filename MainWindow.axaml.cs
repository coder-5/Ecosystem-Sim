using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
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
                ecosystem.activeFood.Add(new FoodSpecies(1, rand.Next(0, 800), rand.Next(0, 450)));
                ecosystem.activeWater.Add(new WaterZone(1, rand.Next(0, 800), rand.Next(0, 450)));
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
            var LineGraphWindow = new LineGraphWindow();
            LineGraphWindow.Show();
            while (true)
            {
                if (!paused)
                {
                    ecosystem.update();
                    EcosystemCanvas.Refresh();
                    LineGraphWindow.drawLineGraph(ecosystem.populationSizes);
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
        }

        public void drawLineGraph(List<double> data)
        {
            GraphCanvas.Children.Clear();

            double width = GraphCanvas.Bounds.Width;
            double height = GraphCanvas.Bounds.Height;
            double xStep = width / (data.Count - 1);
            double yMax = 100;
            double yScale = height / yMax;

            var Polyline = new Polyline
            {
                Stroke = Brushes.Blue,
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