using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentStubbornSteedBehavior : ComponentBehavior, IUpdateable
	{
		private SubsystemTime m_subsystemTime;

		private SubsystemGameInfo m_subsystemGameInfo;

		private ComponentCreature m_componentCreature;

		private ComponentMount m_componentMount;

		private ComponentSteedBehavior m_componentSteedBehavior;

		private ComponentEatPickableBehavior m_componentEatPickableBehavior;

		private StateMachine m_stateMachine = new StateMachine();

		private float m_importanceLevel;

		private bool m_isSaddled;

		private Random m_random = new Random();

		private float m_periodicEventOffset;

		private float m_stubbornProbability;

		private double m_stubbornEndTime;

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override float ImportanceLevel => m_importanceLevel;

		public void Update(float dt)
		{
			m_stateMachine.Update();
			if (!IsActive)
			{
				m_stateMachine.TransitionTo("Inactive");
			}
			if (m_subsystemTime.PeriodicGameTimeEvent(1.0, m_periodicEventOffset))
			{
				if (m_subsystemGameInfo.TotalElapsedGameTime < m_stubbornEndTime && m_componentEatPickableBehavior.Satiation <= 0f && m_componentMount.Rider != null)
				{
					m_importanceLevel = 210f;
				}
				else
				{
					m_importanceLevel = 0f;
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_componentMount = base.Entity.FindComponent<ComponentMount>(throwOnError: true);
			m_componentSteedBehavior = base.Entity.FindComponent<ComponentSteedBehavior>(throwOnError: true);
			m_componentEatPickableBehavior = base.Entity.FindComponent<ComponentEatPickableBehavior>(throwOnError: true);
			m_stubbornProbability = valuesDictionary.GetValue<float>("StubbornProbability");
			m_stubbornEndTime = valuesDictionary.GetValue<double>("StubbornEndTime");
			m_periodicEventOffset = m_random.Float(0f, 100f);
			m_isSaddled = base.Entity.ValuesDictionary.DatabaseObject.Name.EndsWith("_Saddled");
			m_stateMachine.AddState("Inactive", null, delegate
			{
				if (m_subsystemTime.PeriodicGameTimeEvent(1.0, m_periodicEventOffset) && m_componentMount.Rider != null && m_random.Float(0f, 1f) < m_stubbornProbability && (!m_isSaddled || m_componentEatPickableBehavior.Satiation <= 0f))
				{
					m_stubbornEndTime = m_subsystemGameInfo.TotalElapsedGameTime + (double)m_random.Float(60f, 120f);
				}
				if (IsActive)
				{
					m_stateMachine.TransitionTo("Stubborn");
				}
			}, null);
			m_stateMachine.AddState("Stubborn", null, delegate
			{
				if (m_componentSteedBehavior.WasOrderIssued)
				{
					m_componentCreature.ComponentCreatureModel.HeadShakeOrder = m_random.Float(0.6f, 1f);
					m_componentCreature.ComponentCreatureSounds.PlayPainSound();
				}
			}, null);
			m_stateMachine.TransitionTo("Inactive");
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			valuesDictionary.SetValue("StubbornEndTime", m_stubbornEndTime);
		}
	}
}