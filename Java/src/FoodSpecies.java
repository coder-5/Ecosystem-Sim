public class FoodSpecies {
    public int amountOfFood;
    public String speciesName  = "";
    public double xPos;
    public double yPos;
    public  int seedsAmount;
    public double age;
    public double sproutingAge;
    public double seedingAge;
    public double originalSeedingAge;
    public double maxLife;

    public FoodSpecies(int amountOfFood, int xPos, int yPos, int seedsAmount, double sproutingAge, double originalSeedingAge, double maxLife) {
        this.amountOfFood = amountOfFood;
        this.xPos = xPos;
        this.yPos = yPos;
        this.seedsAmount = Math.max(seedsAmount, 0);
        this.sproutingAge = sproutingAge;
        this.originalSeedingAge = originalSeedingAge;
        this.seedingAge = originalSeedingAge;
        this.maxLife = maxLife;
    }
}
