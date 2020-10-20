using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentShapeshifter : Component, IUpdateable
	{
		private SubsystemGameInfo m_subsystemGameInfo;

		private SubsystemSky m_subsystemSky;

		private SubsystemParticles m_subsystemParticles;

		private SubsystemAudio m_subsystemAudio;

		private ComponentSpawn m_componentSpawn;

		private ComponentBody m_componentBody;

		private ComponentHealth m_componentHealth;

		private ShapeshiftParticleSystem m_particleSystem;

		private string m_nightEntityTemplateName;

		private string m_dayEntityTemplateName;

		private float m_timeToSwitch;

		private string m_spawnEntityTemplateName;

		private static Random s_random = new Random();

		public bool IsEnabled
		{
			get;
			set;
		}

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public void Update(float dt)
		{
			bool areSupernaturalCreaturesEnabled = m_subsystemGameInfo.WorldSettings.AreSupernaturalCreaturesEnabled;
			if (IsEnabled && !m_componentSpawn.IsDespawning && m_componentHealth.Health > 0f)
			{
				if (!areSupernaturalCreaturesEnabled && !string.IsNullOrEmpty(m_dayEntityTemplateName))
				{
					ShapeshiftTo(m_dayEntityTemplateName);
				}
				else if (m_subsystemSky.SkyLightIntensity > 0.25f && !string.IsNullOrEmpty(m_dayEntityTemplateName))
				{
					m_timeToSwitch -= 2f * dt;
					if (m_timeToSwitch <= 0f)
					{
						ShapeshiftTo(m_dayEntityTemplateName);
					}
				}
				else if (areSupernaturalCreaturesEnabled && m_subsystemSky.SkyLightIntensity < 0.1f && (m_subsystemSky.MoonPhase == 0 || m_subsystemSky.MoonPhase == 4) && !string.IsNullOrEmpty(m_nightEntityTemplateName))
				{
					m_timeToSwitch -= dt;
					if (m_timeToSwitch <= 0f)
					{
						ShapeshiftTo(m_nightEntityTemplateName);
					}
				}
			}
			if (!string.IsNullOrEmpty(m_spawnEntityTemplateName))
			{
				if (m_particleSystem == null)
				{
					m_particleSystem = new ShapeshiftParticleSystem();
					m_subsystemParticles.AddParticleSystem(m_particleSystem);
				}
				m_particleSystem.BoundingBox = m_componentBody.BoundingBox;
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_subsystemParticles = base.Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_componentSpawn = base.Entity.FindComponent<ComponentSpawn>(throwOnError: true);
			m_componentBody = base.Entity.FindComponent<ComponentBody>(throwOnError: true);
			m_componentHealth = base.Entity.FindComponent<ComponentHealth>(throwOnError: true);
			m_dayEntityTemplateName = valuesDictionary.GetValue<string>("DayEntityTemplateName");
			m_nightEntityTemplateName = valuesDictionary.GetValue<string>("NightEntityTemplateName");
			float value = valuesDictionary.GetValue<float>("Probability");
			if (!string.IsNullOrEmpty(m_dayEntityTemplateName))
			{
				DatabaseManager.FindEntityValuesDictionary(m_dayEntityTemplateName, throwIfNotFound: true);
			}
			if (!string.IsNullOrEmpty(m_nightEntityTemplateName))
			{
				DatabaseManager.FindEntityValuesDictionary(m_nightEntityTemplateName, throwIfNotFound: true);
			}
			m_timeToSwitch = s_random.Float(3f, 15f);
			IsEnabled = (s_random.Float(0f, 1f) < value);
			m_componentSpawn.Despawned += ComponentSpawn_Despawned;
		}

		private void ShapeshiftTo(string entityTemplateName)
		{
			if (string.IsNullOrEmpty(m_spawnEntityTemplateName))
			{
				m_spawnEntityTemplateName = entityTemplateName;
				m_componentSpawn.DespawnDuration = 3f;
				m_componentSpawn.Despawn();
				m_subsystemAudio.PlaySound("Audio/Shapeshift", 1f, 0f, m_componentBody.Position, 3f, autoDelay: true);
			}
		}

		private void ComponentSpawn_Despawned(ComponentSpawn componentSpawn)
		{
			if (m_componentHealth.Health > 0f && !string.IsNullOrEmpty(m_spawnEntityTemplateName))
			{
				Entity entity = DatabaseManager.CreateEntity(base.Project, m_spawnEntityTemplateName, throwIfNotFound: true);
				ComponentBody componentBody = entity.FindComponent<ComponentBody>(throwOnError: true);
				componentBody.Position = m_componentBody.Position;
				componentBody.Rotation = m_componentBody.Rotation;
				componentBody.Velocity = m_componentBody.Velocity;
				entity.FindComponent<ComponentSpawn>(throwOnError: true).SpawnDuration = 0.5f;
				base.Project.AddEntity(entity);
			}
			if (m_particleSystem != null)
			{
				m_particleSystem.Stopped = true;
			}
		}
	}
}
