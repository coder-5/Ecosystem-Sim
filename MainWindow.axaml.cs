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
                ecosystem.activeSpecies.Add(new Species("10:10:1:100:10", "10:10:1:100:10", 100, 100));
                ecosystem.activeSpecies[0].inherit_genes();
                ecosystem.activeFood.Add(new FoodSpecies(1, 150, 150));
                ecosystem.activeWater.Add(new WaterZone(1, 50, 50));
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