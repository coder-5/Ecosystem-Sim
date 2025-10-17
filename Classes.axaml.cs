using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.IO;

namespace EcosystemSim
{
    public class WaterZone
    {
        public float amountOfWater;
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
        public int seedsAmount;
        public float age;
        public float sproutingAge;
        public float seedingAge;
        public float originalSeedingAge;
        public float maxLife;
        public FoodSpecies(int amountOfFoodNew, int posX, int posY, int seedsAmount1, float sproutingAge1, float originalSeedingAge1, float maxLife1) { amountOfFood = amountOfFoodNew; xPos = posX; yPos = posY; seedsAmount = seedsAmount1 >= 0 ? seedsAmount1 : 0; sproutingAge = sproutingAge1; originalSeedingAge = originalSeedingAge1; maxLife = maxLife1; }
    }

    public class Species
    {
        public float stamina = 1;
        public float age = 0;
        public float reproductiveUrge = 0;
        public float hunger = 0;
        public float thirst = 0;
        public string speciesName = "";
        public float xPos;
        public float yPos;
        public float drinkingWaterAmount;
        public enum State
        {
            moving,
            eating,
            drinking,
            nothing
        }

        public State currentState = State.nothing;

        // These variables are for the genes

        public string genes = ""; // first slot: speed, second slot: eye sight, third slot: gender, fourth slot: maxLife, fifth slot: reproductive age. sixth slot is predator, 1 is male 0 is female
        private string[] genesList = new string[2];
        public int speed;
        public int eyeSght;
        public int gender;
        public int maxLife;
        public int reproductiveAge;
        public bool predator;

        public Species(string genesMother, string genesFather, float posX, float posY) { genesList[0] = genesMother; genesList[1] = genesFather; xPos = posX; yPos = posY; }
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
                averageGene += random.Next(-1, 2);
                if (averageGene <= 0)
                {
                    averageGene = 1;
                }
                else if (i == 2)
                {
                    newGenes += random.Next(0, 2) + ":";
                    continue;
                }
                else if (i == motherSplitGenes.Length - 2)
                {
                    newGenes += averageGene.ToString();
                }
                else if (i == motherSplitGenes.Length - 1)
                {
                    if (geneFatherNum == 1)
                    {
                        newGenes += ":" + 1;
                    }
                    else
                    {
                        newGenes += ":" + 0;
                    }
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
                } else if (i == 5)
                {
                    predator = gene == 1 ? true : false;
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
            else if (age >= maxLife)
            {
                return true;
            }
            return false;
        }
        public int wanted_resource()
        {
            if (reproductiveUrge >= thirst && reproductiveUrge >= hunger && age >= reproductiveAge)
            {
                Console.WriteLine($"Species want to mate hunger:{hunger}  thirst:{thirst}   urge to reproduce{reproductiveUrge}  age to reproduce{reproductiveAge}  genes:{genes}");
                return 2;
            }
            if (thirst >= hunger)
            {
                Console.WriteLine($"Species is thirsty hunger:{hunger}  thirst:{thirst}   urge to reproduce{reproductiveUrge}  age to reproduce{reproductiveAge}  genes:{genes}");
                return 0;
            }
            else if (hunger >= thirst)
            {
                Console.WriteLine($"Species is hungry hunger:{hunger}  thirst:{thirst}   urge to reproduce{reproductiveUrge}  age to reproduce:{reproductiveAge}  genes:{genes}");
                return 1;
            }
            return -1;
        }
        public Species mate(Species mate)
        {
            Species child = new Species(genes, mate.genes, xPos + 20, yPos + 20);

            child.inherit_genes();

            reproductiveUrge = 0;

            return child;
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
            age += 0.1f;
            stamina += currentState == State.moving ? -0.05f : 0.01f;
            thirst += (currentState == State.moving ? 0.2f : 0.0f) - (currentState == State.drinking ? drinkingWaterAmount : 0) + speed * 0.01f;
            drinkingWaterAmount = 0;
            hunger += (currentState == State.moving ? 0.05f : 0.01f) - (currentState == State.eating ? 50f : 0) + speed * 0.01f * 0.01f; // make eye sight and stuff effect it and stamina effect it
            if (thirst <= 0)
            {
                thirst = 0;
            }
            if (hunger <= 0)
            {
                hunger = 0;
            }
            reproductiveUrge += age >= reproductiveAge ? 0.01f : 0;
        }
    }
}