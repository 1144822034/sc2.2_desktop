using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentHowlBehavior : ComponentBehavior, IUpdateable
	{
		private SubsystemSky m_subsystemSky;

		private SubsystemAudio m_subsystemAudio;

		private SubsystemTime m_subsystemTime;

		private ComponentCreature m_componentCreature;

		private ComponentPathfinding m_componentPathfinding;

		private StateMachine m_stateMachine = new StateMachine();

		private float m_importanceLevel;

		private string m_howlSoundName;

		private float m_howlTime;

		private float m_howlDuration;

		private Random m_random = new Random();

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override float ImportanceLevel => m_importanceLevel;

		public void Update(float dt)
		{
			m_stateMachine.Update();
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_componentPathfinding = base.Entity.FindComponent<ComponentPathfinding>(throwOnError: true);
			m_howlSoundName = valuesDictionary.GetValue<string>("HowlSoundName");
			m_stateMachine.AddState("Inactive", delegate
			{
				m_importanceLevel = 0f;
			}, delegate
			{
				if (IsActive)
				{
					m_stateMachine.TransitionTo("Howl");
				}
				if (m_subsystemSky.SkyLightIntensity < 0.1f)
				{
					if (m_random.Float(0f, 1f) < 0.015f * m_subsystemTime.GameTimeDelta)
					{
						m_importanceLevel = m_random.Float(1f, 3f);
					}
				}
				else
				{
					m_importanceLevel = 0f;
				}
			}, null);
			m_stateMachine.AddState("Howl", delegate
			{
				m_howlTime = 0f;
				m_howlDuration = m_random.Float(5f, 6f);
				m_componentPathfinding.Stop();
				m_importanceLevel = 10f;
			}, delegate
			{
				if (!IsActive)
				{
					m_stateMachine.TransitionTo("Inactive");
				}
				m_componentCreature.ComponentLocomotion.LookOrder = new Vector2(m_componentCreature.ComponentLocomotion.LookOrder.X, 2f);
				float num = m_howlTime + m_subsystemTime.GameTimeDelta;
				if (m_howlTime <= 0.5f && num > 0.5f)
				{
					m_subsystemAudio.PlayRandomSound(m_howlSoundName, 1f, m_random.Float(-0.1f, 0.1f), m_componentCreature.ComponentBody.Position, 10f, autoDelay: true);
				}
				m_howlTime = num;
				if (m_howlTime >= m_howlDuration)
				{
					m_importanceLevel = 0f;
				}
			}, null);
			m_stateMachine.TransitionTo("Inactive");
		}
	}
}
