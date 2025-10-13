using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

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

            foreach (var food in EcosystemData.activeFood)
            {
                var brush = Brushes.Green;
                var pen = new Pen(Brushes.Transparent, 1);
                var center = new Point(food.xPos, food.yPos);
                context.DrawEllipse(brush, pen, center, size, size);
            }

            foreach (var species in EcosystemData.activeSpecies)
            {
                var brush = Brushes.Black;
                var pen = new Pen(Brushes.Transparent, 1);
                var center = new Point(species.xPos, species.yPos);
                context.DrawEllipse(brush, pen, center, size, size);
            }

            foreach (var water in EcosystemData.activeWater)
            {
                var brush = Brushes.Blue;
                var pen = new Pen(Brushes.Transparent, 1);
                var center = new Point(water.xPos, water.yPos);
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
        public List<Species> activeSpecies = new List<Species>();
        public List<FoodSpecies> activeFood = new List<FoodSpecies>();
        public List<WaterZone> activeWater = new List<WaterZone>();

        public void update()
        {
            foreach (Species species in activeSpecies)
            {
                species.update();
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
                if (species.check_death())
                {
                    activeSpecies.Remove(species);
                }
            }
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
                activeWater.Remove(water);
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
            FoodSpecies result = new FoodSpecies(0, 0, 0);

            float closestDistance = 100000;
            FoodSpecies returnClass = new FoodSpecies(0, 0, 0);
            foreach (FoodSpecies food in activeFood)
            {
                float distance = Vector2.Distance(new Vector2(food.xPos, food.yPos), new Vector2(species.xPos, species.yPos));
                if (distance < closestDistance)
                {
                    returnClass = food;
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
                if (distance < closestDistance)
                {
                    returnClass = water;
                    closestDistance = distance;
                }
            }
            if (closestDistance > species.eyeSght) { return null; }
            return returnClass;
        }
    }
    public class WaterZone
    {
        public int amountOfWater;
        public float xPos;
        public float yPos;
        public WaterZone(int amountOfWaterNew, int posX, int posY) { amountOfWater = amountOfWaterNew; xPos = posX; yPos = posY; }
    }
    public class FoodSpecies
    {
        public int amountOfFood;
        public string speciesName = "";
        public float xPos;
        public float yPos;
        public FoodSpecies(int amountOfFoodNew, int posX, int posY) { amountOfFood = amountOfFoodNew; xPos = posX; yPos = posY; }
    }

    public class Species
    {
        public int stamina = 1;
        public int age = 0;
        public int reproductiveUrge = 0;
        public int hunger = 0;
        public int thirst = 0;
        public string speciesName = "";
        public float xPos;
        public float yPos;
        public enum State
        {
            moving,
            eating,
            drinking,
            nothing
        }

        public State currentState = State.nothing;

        // These variables are for the genes

        public string genes = ""; // first slot: speed, second slot: eye sight, third slot: gender, fourth slot: maxLife, fifth slot: reproductive age
        private string[] genesList = new string[2];
        public int speed;
        public int eyeSght;
        public int gender;
        public int maxLife;
        public int reproductiveAge;

        public Species(string genesMother, string genesFather, int posX, int posY) { genesList[0] = genesMother; genesList[1] = genesFather; xPos = posX; yPos = posY; }
        public void inherit_genes()
        {
            Random random = new Random();
            genes = "";
            string newGenes = "";
            string[] motherSplitGenes = genesList[0].Split(':');
            string[] fatherSplitGenes = genesList[1].Split(':');
            for (int i = 0; i < motherSplitGenes.Length; i++)
            {
                int geneMotherNum = int.Parse(motherSplitGenes[i]);
                int geneFatherNum = int.Parse(fatherSplitGenes[i]);
                int averageGene = (geneMotherNum + geneFatherNum) / 2;
                averageGene += random.Next(-1, 1);
                if (averageGene <= 0)
                {
                    averageGene = 1;
                }
                if (i == motherSplitGenes.Length - 1)
                {
                    newGenes += averageGene.ToString();
                }
                else
                {
                    newGenes += averageGene.ToString() + ":";
                }
            }
            // can add checks here to make sure it isnt corrupted
            genes = newGenes;
            initialize_genes();
        }
        public void initialize_genes()
        {
            string[] splitGenes = genes.Split(':');
            for (int i = 0; i < splitGenes.Length; i++)
            {
                int gene = int.Parse(splitGenes[i]);
                if (i == 0)
                {
                    speed = gene;
                }
                else if (i == 1)
                {
                    eyeSght = gene;
                }
                else if (i == 2)
                {
                    gender = gene;
                }
                else if (i == 3)
                {
                    maxLife = gene;
                }
                else if (i == 4)
                {
                    reproductiveAge = gene;
                }
            }
        }
        public bool check_death()
        {
            if (hunger >= 100)
            {
                return true;
            }
            else if (thirst >= 100)
            {
                return true;
            }
            return false;
        }
        public int wanted_resource()
        {
            if (thirst >= hunger)
            {
                return 0;
            }
            else if (hunger >= thirst)
            {
                return 1;
            }
            else if (reproductiveUrge >= thirst && reproductiveUrge >= hunger && age >= reproductiveAge)
            {
                return 2;
            }
            return -1;
        }
        public bool move_species(Vector2 targetPos) // type 1 is water, type 2 is food, type 3 is mate
        {
            currentState = State.moving;

            float angle = MathF.Atan2(targetPos.Y - yPos, targetPos.X - xPos);

            float dx = MathF.Cos(angle);
            float dy = MathF.Sin(angle);

            Vector2 direction = new Vector2(dx, dy);
            Vector2 newPos = new Vector2(xPos, yPos) + direction * this.speed;

            this.xPos = newPos.X;
            this.yPos = newPos.Y;

            float distance = Vector2.Distance(targetPos, newPos);

            if (distance <= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void update()
        {
            age += 1;
            stamina += currentState == State.moving ? -5 : 1;
            thirst += (currentState == State.moving ? 10 : 1) - (currentState == State.drinking ? -25 : 0);
            hunger += (currentState == State.moving ? 5 : 1) - (currentState == State.eating ? 10 : 0);
            reproductiveUrge += age >= reproductiveAge ? 5 : 0;
        }
    }
}