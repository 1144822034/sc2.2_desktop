using Engine;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game
{
	public class RecipaediaRecipesScreen : Screen
	{
		private CraftingRecipeWidget m_craftingRecipeWidget;

		private SmeltingRecipeWidget m_smeltingRecipeWidget;

		private ButtonWidget m_prevRecipeButton;

		private ButtonWidget m_nextRecipeButton;

		private int m_recipeIndex;

		private List<CraftingRecipe> m_craftingRecipes = new List<CraftingRecipe>();

		public RecipaediaRecipesScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/RecipaediaRecipesScreen");
			LoadContents(this, node);
			m_craftingRecipeWidget = Children.Find<CraftingRecipeWidget>("CraftingRecipe");
			m_smeltingRecipeWidget = Children.Find<SmeltingRecipeWidget>("SmeltingRecipe");
			m_prevRecipeButton = Children.Find<ButtonWidget>("PreviousRecipe");
			m_nextRecipeButton = Children.Find<ButtonWidget>("NextRecipe");
		}

		public override void Enter(object[] parameters)
		{
			int value = (int)parameters[0];
			m_craftingRecipes.Clear();
			m_craftingRecipes.AddRange(CraftingRecipesManager.Recipes.Where((CraftingRecipe r) => r.ResultValue == value && r.ResultValue != 0));
			m_recipeIndex = 0;
		}

		public override void Update()
		{
			if (m_recipeIndex < m_craftingRecipes.Count)
			{
				CraftingRecipe craftingRecipe = m_craftingRecipes[m_recipeIndex];
				if (craftingRecipe.RequiredHeatLevel == 0f)
				{
					m_craftingRecipeWidget.Recipe = craftingRecipe;
					m_craftingRecipeWidget.NameSuffix = $" (recipe #{m_recipeIndex + 1})";
					m_craftingRecipeWidget.IsVisible = true;
					m_smeltingRecipeWidget.IsVisible = false;
				}
				else
				{
					m_smeltingRecipeWidget.Recipe = craftingRecipe;
					m_smeltingRecipeWidget.NameSuffix = $" (recipe #{m_recipeIndex + 1})";
					m_smeltingRecipeWidget.IsVisible = true;
					m_craftingRecipeWidget.IsVisible = false;
				}
			}
			m_prevRecipeButton.IsEnabled = (m_recipeIndex > 0);
			m_nextRecipeButton.IsEnabled = (m_recipeIndex < m_craftingRecipes.Count - 1);
			if (m_prevRecipeButton.IsClicked)
			{
				m_recipeIndex = MathUtils.Max(m_recipeIndex - 1, 0);
			}
			if (m_nextRecipeButton.IsClicked)
			{
				m_recipeIndex = MathUtils.Min(m_recipeIndex + 1, m_craftingRecipes.Count - 1);
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
