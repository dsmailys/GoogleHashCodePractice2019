namespace pizzaSolution
{
    public class PizzaIngredient
    {
        public PizzaIngredient (Ingredient ingredient) {
            Ingredient = ingredient;
        }
        
        public Ingredient Ingredient { get; }
    }

    public enum Ingredient {
        Mushroom,
        Tomatoe
    }
}