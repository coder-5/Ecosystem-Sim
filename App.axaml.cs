using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace EcosystemSim;

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

public class Ecosystem
{
    public List<Species> activeSpecies = new List<Species>();
    public List<FoodSpecies> activeFood = new List<FoodSpecies>();

    public void update()
    {
        foreach (Species specie in activeSpecies)
        {
            specie.update();
            if (specie.wanted_resource() == 0)
            {
                
            }
        }
    }
}
public class FoodSpecies
{
    public int amountOfFood;
    public string speciesName = "";
    public int xPos;
    public int yPos;
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
    public int xPos;
    public int yPos;
    public enum State
    {
        moving,
        eating,
        drinking,
        nothing
    }

    State currentState = State.nothing;

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
            newGenes += averageGene.ToString() + ":";
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
    public void update()
    {
        age += 1;
        stamina += currentState == State.moving ? -5 : 1;
        thirst += (currentState == State.moving ? 10 : 1) - (currentState == State.drinking ? 25 : 0);
        hunger += (currentState == State.moving ? 5 : 1) - (currentState == State.eating ? 10 : 0);
        reproductiveUrge += age >= reproductiveAge ? 5 : 0;
        check_death();
    }
}