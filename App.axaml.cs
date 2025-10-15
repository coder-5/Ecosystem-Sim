using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Linq;

namespace EcosystemSim
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
    public class EcosystemView : Control
    {
        public Ecosystem EcosystemData { get; set; }
        public float size = 5f;

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (EcosystemData == null) Debug.WriteLine("Ecosystem Data is null");

            foreach (var water in EcosystemData.activeWater)
            {
                var brush = water.amountOfWater == 0 ? Brushes.Transparent : water.amountOfWater >= 0 && water.amountOfWater <= 25 ? Brushes.LightBlue : water.amountOfWater >= 26 && water.amountOfWater <= 75 ? Brushes.Blue : Brushes.DarkBlue;
                var pen = new Pen(Brushes.Transparent, 1);
                var center = new Point(water.xPos, water.yPos);
                context.DrawEllipse(brush, pen, center, size, size);
            }

            foreach (var food in EcosystemData.activeFood)
            {
                var brush = food.age >= food.sproutingAge ? Brushes.Green : Brushes.Brown;
                var pen = new Pen(Brushes.Transparent, 1);
                var center = new Point(food.xPos, food.yPos);
                context.DrawEllipse(brush, pen, center, size, size);
            }

            foreach (var species in EcosystemData.activeSpecies)
            {
                var brush = Brushes.Red;
                var pen = new Pen(Brushes.Transparent, 1);
                var center = new Point(species.xPos, species.yPos);
                context.DrawEllipse(brush, pen, center, size, size);
            }
        }
        public void Refresh()
        {
            InvalidateVisual();
        }
    }

    public class Ecosystem
    {
        static Random random = new Random();
        static string start_time = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        public List<Species> activeSpecies { get; set; } = new();
        public List<FoodSpecies> activeFood { get; set; } = new();
        public List<WaterZone> activeWater { get; set; } = new();
        public List<double> populationSizes = new();
        public List<double> foodSizes = new();
        public List<double> maleSpecies = new();
        public List<double> femaleSpecies = new();
        public List<double> sproutedPlants = new();
        public List<double> unSproutedPlants = new();
        public List<double> averageSpeedPrey = new();
        public List<double> averageReproductionAge = new();
        public List<double> averageEyeSight = new();
        public bool devBeta = true;

        public void start()
        {
            Directory.CreateDirectory("saves/" + start_time);
        }
        public void update()
        {
            maleSpecies.Add(0);
            femaleSpecies.Add(0);
            List<double> speed = new();
            List<double> eyeSisht = new();
            List<double> reproductionAge = new();
            Console.WriteLine("==========Update==========");
            for (int i = activeSpecies.Count - 1; i >= 0; i--)
            {
                Species species = activeSpecies[i];
                speed.Add(species.speed);
                averageEyeSight.Add(species.eyeSght);
                averageReproductionAge.Add(species.reproductiveAge);
                if (species.gender == 0)
                {
                    femaleSpecies[femaleSpecies.Count - 1] += 1;
                }
                else
                {
                    maleSpecies[maleSpecies.Count - 1] += 1;
                }
                Console.WriteLine($"Species num:{i} is age: {species.age}");
                species.update();
                int wanted = species.wanted_resource();
                if (species.wanted_resource() == 0)
                {
                    bool worked = goToWater(species);
                    if (!worked)
                    {
                        goToFood(species);
                    }
                }
                else if (species.wanted_resource() == 1)
                {
                    bool worked = goToFood(species);
                }
                else if (species.wanted_resource() == 2)
                {
                    int worked = goToSpecies(species);
                    if (worked == 0)
                    {
                        goToFood(species);
                    }
                }
                if (species.check_death())
                {
                    activeSpecies.RemoveAt(i);
                }
            }

            averageSpeedPrey.Add(speed.Average());
            averageEyeSight.Add(eyeSisht.Average());
            averageReproductionAge.Add(reproductionAge.Average());

            populationSizes.Add(activeSpecies.Count);
            foodSizes.Add(activeFood.Count);
            foreach (var water in activeWater)
            {
                water.amountOfWater += 1f;
            }
            List<FoodSpecies> newFoods = new();
            sproutedPlants.Add(0);
            unSproutedPlants.Add(0);
            foreach (var food in activeFood)
            {
                food.age += 1f;
                if (food.age >= food.seedingAge)
                {
                    food.seedingAge += food.originalSeedingAge;
                    int spawnCount = food.amountOfFood + 1;
                    for (int i = 0; i < spawnCount; i++)
                    {

                        FoodSpecies newFood;

                        bool validPosition = false;

                        int attempts = 0;

                        do
                        {
                            float x = Math.Clamp(food.xPos + random.Next(-150, 150), 0, 800);
                            float y = Math.Clamp(food.yPos + random.Next(-150, 150), 0, 450);
                            newFood = new FoodSpecies(1, (int)x, (int)y, food.seedsAmount + random.Next(-1, 2), food.sproutingAge + random.Next(-1, 2), food.originalSeedingAge + random.Next(-1, 2), food.maxLife + random.Next(-1,2));
                            newFood.seedingAge = newFood.originalSeedingAge;

                            validPosition = true;
                            foreach (var existing in activeFood)
                            {
                                if (Vector2.Distance(new Vector2(existing.xPos, existing.yPos), new Vector2(x, y)) <= 10)
                                {
                                    validPosition = false;
                                    break;
                                }
                            }
                            foreach (var existing in activeWater)
                            {
                                if (Vector2.Distance(new Vector2(existing.xPos, existing.yPos), new Vector2(x, y)) <= 20)
                                {
                                    validPosition = false;
                                    break;
                                }
                            }
                            attempts++;
                        }
                        while (!validPosition && attempts < 20);

                        if (validPosition)
                        {
                            newFoods.Add(newFood);
                        }
                    }
                }
                if (food.age >= food.sproutingAge)
                {
                    sproutedPlants[sproutedPlants.Count - 1] += 1;
                }
                else
                {
                    unSproutedPlants[unSproutedPlants.Count - 1] += 1;
                }
            }
            foreach (var food1 in newFoods)
            {
                activeFood.Add(food1);
            }
            for (int i = activeFood.Count - 1; i >= 0; i--)
            {
                if (activeFood[i].age >= activeFood[i].maxLife)
                {
                    activeFood.RemoveAt(i);
                }
            }
            //update_text();
            if (!devBeta) { saveToJson(); }
        }
        public void saveToJson(string filename = "")
        {
            Console.WriteLine("Saving to json called");

            try
            {
                Console.WriteLine("Saving to json file");
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true
                };

                if (string.IsNullOrEmpty(filename))
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    filename = $"saves/{start_time}/date_{timestamp}.json";
                }

                string json = JsonSerializer.Serialize(this, options); // finish the json serialization code and such
                File.WriteAllText(filename, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving json: " + ex);
            }
        }
        public void update_text()
        {
            List<string> text = new List<string>();

            text.Add("=====UPDATE=====");

            for (int i = 0; i < activeSpecies.Count; i++)
            {
                Species species = activeSpecies[i];
                text.Add($"current species {i} hunger:{species.hunger}  thirst:{species.thirst}   urge to reproduce{species.reproductiveUrge}  age to reproduce:{species.reproductiveAge}  genes:{species.genes}   xPos:{species.xPos}   yPos:{species.yPos}   state:{species.currentState}   eye Sight:{species.eyeSght}   gender:{species.gender}");
            }

            File.AppendAllLinesAsync("data.txt", text);
        }
        public int goToSpecies(Species species)
        {
            Species species1 = FindClosestOfTypeSpecies(species);

            if (species1 == null)
            {
                double radius = 100.0;

                (double x, double y) = RandomPointInCircle(radius, new Vector2(species.xPos, species.yPos));

                Vector2 currentPosX = new Vector2(species.xPos, species.yPos);
                Vector2 targetPosX = new Vector2((float)x, (float)y);

                species.move_species(targetPosX);

                return 0;
            }

            Vector2 currentPos = new Vector2(species.xPos, species.yPos);
            Vector2 targetPos = new Vector2(species1.xPos, species1.yPos);

            bool collided = species.move_species(targetPos);

            if (collided && species.gender == 0)
            {
                species.currentState = Species.State.nothing;
                activeSpecies.Add(species.mate(species1));
                return 2;
            }

            return 1;
            // x is cosin, y is sin
        }
        public bool goToFood(Species species)
        {
            FoodSpecies food = FindClosestOfTypeFood(species);

            if (food == null)
            {
                double radius = 100.0;

                (double x, double y) = RandomPointInCircle(radius, new Vector2(species.xPos, species.yPos));

                Vector2 currentPosX = new Vector2(species.xPos, species.yPos);
                Vector2 targetPosX = new Vector2((float)x, (float)y);

                species.move_species(targetPosX);

                return false;
            }

            Vector2 currentPos = new Vector2(species.xPos, species.yPos);
            Vector2 targetPos = new Vector2(food.xPos, food.yPos);

            bool collided = species.move_species(targetPos);

            if (collided)
            {
                species.currentState = Species.State.eating;

                activeFood.Remove(food);
            }

            return true;
            // x is cosin, y is sin
        }
        public bool goToWater(Species species)
        {
            WaterZone water = FindClosestWaterZone(species);

            if (water == null)
            {
                double radius = 100.0;

                (double x, double y) = RandomPointInCircle(radius, new Vector2(species.xPos, species.yPos));

                Vector2 currentPosX = new Vector2(species.xPos, species.yPos);
                Vector2 targetPosX = new Vector2((float)x, (float)y);

                species.move_species(targetPosX);

                return false;
            }

            Vector2 currentPos = new Vector2(species.xPos, species.yPos);
            Vector2 targetPos = new Vector2(water.xPos, water.yPos);

            bool collided = species.move_species(targetPos);

            if (collided)
            {
                species.currentState = Species.State.drinking;
                species.drinkingWaterAmount = water.amountOfWater;
                water.amountOfWater = 0;
            }

            return true;
            // x is cosin, y is sin
        }
        static (double, double) RandomPointInCircle(double radius, Vector2 offset)
        {
            double angle = random.NextDouble() * MathF.PI * 2;
            double distance = Math.Sqrt(random.NextDouble()) * radius;

            double x = Math.Cos(angle) * distance;
            double y = Math.Sin(angle) * distance;

            x += offset.X;
            y += offset.Y;

            return (x, y);
        }
        public FoodSpecies FindClosestOfTypeFood(Species species)
        {
            FoodSpecies result = new FoodSpecies(0, 0, 0, 1, 0, 0, 0);

            float closestDistance = 100000;
            FoodSpecies returnClass = new FoodSpecies(0, 0, 0, 1, 0, 0, 0);
            foreach (FoodSpecies food in activeFood)
            {
                float distance = Vector2.Distance(new Vector2(food.xPos, food.yPos), new Vector2(species.xPos, species.yPos));
                if (distance < closestDistance && food.age >= food.sproutingAge)
                {
                    returnClass = food;
                    closestDistance = distance;
                }
            }
            if (closestDistance > species.eyeSght) { return null; }
            return returnClass;
        }
        public Species FindClosestOfTypeSpecies(Species species)
        {
            float closestDistance = 100000;
            Species returnClass = new Species("", "", 0, 0);
            foreach (Species species1 in activeSpecies)
            {
                float distance = Vector2.Distance(new Vector2(species1.xPos, species1.yPos), new Vector2(species.xPos, species.yPos));
                if (distance < closestDistance && species1.gender != species.gender)
                {
                    returnClass = species1;
                    closestDistance = distance;
                }
            }
            if (closestDistance > species.eyeSght) { return null; }
            return returnClass;
        }
        public WaterZone FindClosestWaterZone(Species species)
        {
            WaterZone result = new WaterZone(0, 0, 0);

            float closestDistance = 100000;
            WaterZone returnClass = new WaterZone(0, 0, 0);
            foreach (WaterZone water in activeWater)
            {
                float distance = Vector2.Distance(new Vector2(water.xPos, water.yPos), new Vector2(species.xPos, species.yPos));
                if (distance < closestDistance && water.amountOfWater > 0)
                {
                    returnClass = water;
                    closestDistance = distance;
                }
            }
            if (closestDistance > species.eyeSght) { return null; }
            return returnClass;
        }
    }
}