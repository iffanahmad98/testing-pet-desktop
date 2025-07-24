namespace MagicalGarden.Farm
{
    public class Monster {
        public string name;
        public MonsterType rarity;
        public int power;
        public string element;

        public Monster(string name, MonsterType rarity) {
            this.name = name;
            this.rarity = rarity;
            this.power = (int)rarity * 100 + 100; // Just an example formula
            this.element = "Neutral"; // Placeholder
        }
    }
}