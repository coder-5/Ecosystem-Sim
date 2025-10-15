using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Data;
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
            for (int i = 0; i < 51; i++)
            {
                ecosystem.activeFood.Add(new FoodSpecies(1, rand.Next(0, 800), rand.Next(0, 450), rand.Next(1, 4), 50, 500 + rand.Next(-50,51), 1000 + rand.Next(-50,51)));
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
            var populationLineGraph = new LineGraphWindow("Populations Graph");
            populationLineGraph.Show();
            var femaleToMale = new LineGraphWindow("Female v Male Line Graph");
            femaleToMale.Show();
            var sproutedToUnsprouted = new LineGraphWindow("Sprouted v Un-Sprouted Line Graph");
            sproutedToUnsprouted.Show();
            while (true)
            {
                if (!paused)
                {
                    ecosystem.update();
                    EcosystemCanvas.Refresh();
                    List<IBrush> colors = [Brushes.Red, Brushes.Green];
                    populationLineGraph.drawLineGraph(new List<List<double>> { ecosystem.populationSizes, ecosystem.foodSizes }, colors, new List<string> { "Population Size", "Food Population"});
                    List<IBrush> colors2 = [Brushes.Red, Brushes.Black];
                    femaleToMale.drawLineGraph(new List<List<double>> { ecosystem.femaleSpecies, ecosystem.maleSpecies }, colors2, new List<string> { "Female", "Male"});
                    List<IBrush> colors3 = [Brushes.Green, Brushes.Brown];
                    sproutedToUnsprouted.drawLineGraph(new List<List<double>> { ecosystem.sproutedPlants, ecosystem.unSproutedPlants }, colors3, new List<string> { "Sprouted", "UnSprouted"});
                    List<IBrush> colors4 = [Brushes.Green, Brushes.Blue, Brushes.Red];
                    sproutedToUnsprouted.drawLineGraph(new List<List<double>> { ecosystem.averageEyeSight, ecosystem.averageReproductionAge, ecosystem.averageSpeedPrey }, colors4, new List<string> { "Eye Sight", "Reproduction", "Speed"});
                }
                await Task.Delay(10);
            }
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