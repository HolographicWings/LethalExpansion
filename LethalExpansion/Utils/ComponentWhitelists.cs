using LethalSDK.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Video;
using UnityEngine;
using LethalSDK.Utils;

namespace LethalExpansion.Utils
{
    public class ComponentWhitelists
    {
        public static List<Type> moonPrefabWhitelist = new List<Type> {
            //Base
            typeof(Transform),
            //Mesh
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            //Physics
            typeof(MeshCollider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(SphereCollider),
            typeof(TerrainCollider),
            typeof(WheelCollider),
            typeof(ArticulationBody),
            typeof(ConstantForce),
            typeof(ConfigurableJoint),
            typeof(FixedJoint),
            typeof(HingeJoint),
            typeof(Cloth),
            typeof(Rigidbody),
            //Netcode
            typeof(NetworkObject),
            typeof(NetworkRigidbody),
            typeof(NetworkTransform),
            typeof(NetworkAnimator),
            //Animation
            typeof(Animator),
            typeof(Animation),
            //Terrain
            typeof(Terrain),
            typeof(Tree),
            typeof(WindZone),
            //Rendering
            typeof(DecalProjector),
            typeof(LODGroup),
            typeof(Light),
            typeof(HDAdditionalLightData),
            typeof(LightProbeGroup),
            typeof(LightProbeProxyVolume),
            typeof(LocalVolumetricFog),
            typeof(OcclusionArea),
            typeof(OcclusionPortal),
            typeof(ReflectionProbe),
            typeof(PlanarReflectionProbe),
            typeof(HDAdditionalReflectionData),
            typeof(Skybox),
            typeof(SortingGroup),
            typeof(SpriteRenderer),
            typeof(Volume),
            //Audio
            typeof(AudioSource),
            typeof(AudioReverbZone),
            typeof(AudioReverbFilter),
            typeof(AudioChorusFilter),
            typeof(AudioDistortionFilter),
            typeof(AudioEchoFilter),
            typeof(AudioHighPassFilter),
            typeof(AudioLowPassFilter),
            typeof(AudioListener),
            //Effect
            typeof(LensFlare),
            typeof(TrailRenderer),
            typeof(LineRenderer),
            typeof(ParticleSystem),
            typeof(ParticleSystemRenderer),
            typeof(ParticleSystemForceField),
            typeof(Projector),
            //Video
            typeof(VideoPlayer),
            //Navigation
            typeof(NavMeshSurface),
            typeof(NavMeshModifier),
            typeof(NavMeshModifierVolume),
            typeof(NavMeshLink),
            typeof(NavMeshObstacle),
            typeof(OffMeshLink),
            //LethalSDK
            typeof(SI_AudioReverbPresets),
            typeof(SI_AudioReverbTrigger),
            typeof(SI_DungeonGenerator),
            typeof(SI_MatchLocalPlayerPosition),
            typeof(SI_AnimatedSun),
            typeof(SI_EntranceTeleport),
            typeof(SI_ScanNode),
            typeof(SI_DoorLock),
            typeof(SI_WaterSurface),
            typeof(SI_Ladder),
            typeof(SI_ItemDropship),
            typeof(SI_NetworkPrefabInstancier),
            typeof(SI_InteractTrigger),
            typeof(SI_DamagePlayer),
            typeof(SI_SoundYDistance),
            typeof(SI_AudioOutputInterface),
            typeof(SI_NetworkDataInterfacing),
            typeof(SI_NetworkData),
            typeof(PlayerShip)
        };
        public static List<Type> scrapWhitelist = new List<Type> {
            //Base
            typeof(Transform),
            //Mesh
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            //Physics
            typeof(MeshCollider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(SphereCollider),
            typeof(TerrainCollider),
            typeof(WheelCollider),
            typeof(ArticulationBody),
            typeof(ConstantForce),
            typeof(ConfigurableJoint),
            typeof(FixedJoint),
            typeof(HingeJoint),
            typeof(Cloth),
            typeof(Rigidbody),
            //Netcode
            typeof(NetworkObject),
            typeof(NetworkRigidbody),
            typeof(NetworkTransform),
            typeof(NetworkAnimator),
            //Animation
            typeof(Animator),
            typeof(Animation),
            //Rendering
            typeof(DecalProjector),
            typeof(LODGroup),
            typeof(Light),
            typeof(HDAdditionalLightData),
            typeof(LightProbeGroup),
            typeof(LightProbeProxyVolume),
            typeof(LocalVolumetricFog),
            typeof(OcclusionArea),
            typeof(OcclusionPortal),
            typeof(ReflectionProbe),
            typeof(PlanarReflectionProbe),
            typeof(HDAdditionalReflectionData),
            typeof(SortingGroup),
            typeof(SpriteRenderer),
            //Audio
            typeof(AudioSource),
            typeof(AudioReverbZone),
            typeof(AudioReverbFilter),
            typeof(AudioChorusFilter),
            typeof(AudioDistortionFilter),
            typeof(AudioEchoFilter),
            typeof(AudioHighPassFilter),
            typeof(AudioLowPassFilter),
            typeof(AudioListener),
            //Effect
            typeof(LensFlare),
            typeof(TrailRenderer),
            typeof(LineRenderer),
            typeof(ParticleSystem),
            typeof(ParticleSystemRenderer),
            typeof(ParticleSystemForceField),
            //Video
            typeof(VideoPlayer),
            //LethalSDK
            typeof(SI_DamagePlayer),
            typeof(SI_AudioOutputInterface),
            typeof(SI_NetworkDataInterfacing),
            typeof(SI_NetworkData)
        };
    }
}
