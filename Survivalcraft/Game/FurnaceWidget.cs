using Engine;
using System.Xml.Linq;

namespace Game
{
	public class FurnaceWidget : CanvasWidget
	{
		private GridPanelWidget m_inventoryGrid;

		private GridPanelWidget m_furnaceGrid;

		private InventorySlotWidget m_fuelSlot;

		private InventorySlotWidget m_resultSlot;

		private InventorySlotWidget m_remainsSlot;

		private FireWidget m_fire;

		private ValueBarWidget m_progress;

		private ComponentFurnace m_componentFurnace;

		public FurnaceWidget(IInventory inventory, ComponentFurnace componentFurnace)
		{
			m_componentFurnace = componentFurnace;
			XElement node = ContentManager.Get<XElement>("Widgets/FurnaceWidget");
			LoadContents(this, node);
			m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
			m_furnaceGrid = Children.Find<GridPanelWidget>("FurnaceGrid");
			m_fire = Children.Find<FireWidget>("Fire");
			m_progress = Children.Find<ValueBarWidget>("Progress");
			m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
			m_remainsSlot = Children.Find<InventorySlotWidget>("RemainsSlot");
			m_fuelSlot = Children.Find<InventorySlotWidget>("FuelSlot");
			int num = 10;
			for (int i = 0; i < m_inventoryGrid.RowsCount; i++)
			{
				for (int j = 0; j < m_inventoryGrid.ColumnsCount; j++)
				{
					InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
					inventorySlotWidget.AssignInventorySlot(inventory, num++);
					m_inventoryGrid.Children.Add(inventorySlotWidget);
					m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
				}
			}
			num = 0;
			for (int k = 0; k < m_furnaceGrid.RowsCount; k++)
			{
				for (int l = 0; l < m_furnaceGrid.ColumnsCount; l++)
				{
					InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
					inventorySlotWidget2.AssignInventorySlot(componentFurnace, num++);
					m_furnaceGrid.Children.Add(inventorySlotWidget2);
					m_furnaceGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
				}
			}
			m_fuelSlot.AssignInventorySlot(componentFurnace, componentFurnace.FuelSlotIndex);
			m_resultSlot.AssignInventorySlot(componentFurnace, componentFurnace.ResultSlotIndex);
			m_remainsSlot.AssignInventorySlot(componentFurnace, componentFurnace.RemainsSlotIndex);
		}

		public override void Update()
		{
			m_fire.ParticlesPerSecond = ((m_componentFurnace.HeatLevel > 0f) ? 24 : 0);
			m_progress.Value = m_componentFurnace.SmeltingProgress;
			if (!m_componentFurnace.IsAddedToProject)
			{
				base.ParentWidget.Children.Remove(this);
			}
		}
	}
}
