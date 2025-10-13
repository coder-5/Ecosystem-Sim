using Avalonia.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EcosystemSim
{
    public partial class MainWindow : Window
    {
        Random rand = new Random();
        Ecosystem ecosystem = new Ecosystem();
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 10; i++)
            {
                ecosystem.activeSpecies.Add(new Species("5:500:1:100:10", "5:500:0:100:10", rand.Next(0, 800), rand.Next(0, 450)));
                ecosystem.activeSpecies[i].inherit_genes();
                ecosystem.activeFood.Add(new FoodSpecies(1, rand.Next(0, 800), rand.Next(0, 450)));
                ecosystem.activeWater.Add(new WaterZone(1, rand.Next(0, 800), rand.Next(0, 450)));
            }

            EcosystemCanvas.EcosystemData = ecosystem;

            RunLoop();
        }

        private async void RunLoop()
        {
            while (true)
            {
                ecosystem.update();
                EcosystemCanvas.Refresh();
                await Task.Delay(100);
            }
        }
    }
}