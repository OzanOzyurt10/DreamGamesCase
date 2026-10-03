using System.Collections.Generic;
using DreamCase.Core.Items;
using DreamCase.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DreamCase.Editor
{
    /// <summary>
    /// One-time tool: creates the particle effect prefabs from the art pack, assigns them to the item prefabs, and
    /// adds the effect pool to LevelScene. Delete this file after running it once.
    /// Sizes, speeds and radii are in cells: effects are scaled to the board's cell size when they play.
    /// </summary>
    public static class EffectsSetup
    {
        private const string ArtRoot = "Assets/Art/";
        private const string EffectsFolder = "Assets/Prefabs/Effects";
        private const string MaterialsFolder = "Assets/Materials/Particles";
        private const string ItemPrefabsFolder = "Assets/Prefabs/Items/";
        private const string LevelScenePath = "Assets/Scenes/LevelScene.unity";

        // Above every item and rocket half, below the top panel (200).
        private const int EffectSortingOrder = 150;
        // Just below the rocket halves (100), so a half flies in front of its own trail.
        private const int TrailSortingOrder = 99;

        private sealed class Style
        {
            public bool Additive;
            public Vector2 Lifetime = new Vector2(0.45f, 0.75f);
            public Vector2 Speed = new Vector2(2.5f, 5f);
            public float Gravity = 0.8f;
            public float Spin = 6f;
            public float Radius = 0.25f;
            public Color Color = Color.white;
            public bool Grows;
            public float FadeFrom = 0.6f;
        }

        /// <summary>One sprite of an effect: how many particles it emits, and its size per 100 px of texture width.</summary>
        private readonly struct Part
        {
            public readonly string Texture;
            public readonly int Count;
            public readonly float SizePer100Px;
            public readonly Style Style;

            public Part(string texture, int count, float sizePer100Px, Style style)
            {
                Texture = texture;
                Count = count;
                SizePer100Px = sizePer100Px;
                Style = style;
            }
        }

        private static readonly Style Pieces = new Style();
        private static readonly Style BigPieces = new Style { Radius = 0.8f };
        private static readonly Style Flash = new Style
        {
            Additive = true, Lifetime = new Vector2(0.3f, 0.4f), Speed = Vector2.zero, Gravity = 0f, Spin = 0f,
            Radius = 0f, Color = new Color(1f, 0.75f, 0.3f), Grows = true, FadeFrom = 0.3f,
        };
        private static readonly Style Fire = new Style
        {
            Additive = true, Lifetime = new Vector2(0.35f, 0.6f), Speed = new Vector2(3f, 7f), Gravity = 0f, Spin = 2f,
            Radius = 0.3f, Color = new Color(1f, 0.6f, 0.2f), FadeFrom = 0.4f,
        };
        private static readonly Style Smoke = new Style
        {
            Lifetime = new Vector2(0.35f, 0.55f), Speed = new Vector2(0.5f, 1.5f), Gravity = 0f, Spin = 1f,
            Radius = 0.2f, Color = new Color(1f, 1f, 1f, 0.8f), Grows = true, FadeFrom = 0.3f,
        };
        private static readonly Style TrailSmoke = new Style
        {
            Lifetime = new Vector2(0.25f, 0.35f), Speed = new Vector2(0f, 0.3f), Gravity = 0f, Spin = 1f,
            Radius = 0.05f, Color = new Color(1f, 1f, 1f, 0.7f), Grows = true, FadeFrom = 0.3f,
        };
        private static readonly Style TrailSparks = new Style
        {
            Additive = true, Lifetime = new Vector2(0.25f, 0.4f), Speed = new Vector2(0.5f, 1.5f), Gravity = 0f,
            Spin = 4f, Radius = 0.1f, Color = new Color(1f, 0.9f, 0.4f), FadeFrom = 0.5f,
        };

        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("Dream Games/One-time Setup/Build Effects")]
        private static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(LevelScenePath);
            if (Object.FindFirstObjectByType<EffectPool>() != null)
            {
                EditorUtility.DisplayDialog("Effects Setup", "LevelScene already has an EffectPool. Nothing was changed.", "OK");
                return;
            }

            EnsureFolder("Assets/Prefabs", "Effects");
            EnsureFolder("Assets/Materials", "Particles");
            Materials.Clear();

            var cubeEffects = new Dictionary<CubeColor, ParticleSystem>
            {
                [CubeColor.Red] = CreateBurst("CubeRed", new Part("Cubes/Particles/particle_red.png", 6, 0.18f, Pieces)),
                [CubeColor.Green] = CreateBurst("CubeGreen", new Part("Cubes/Particles/particle_green.png", 6, 0.18f, Pieces)),
                [CubeColor.Blue] = CreateBurst("CubeBlue", new Part("Cubes/Particles/particle_blue.png", 6, 0.18f, Pieces)),
                [CubeColor.Yellow] = CreateBurst("CubeYellow", new Part("Cubes/Particles/particle_yellow.png", 6, 0.18f, Pieces)),
            };

            var vase = CreateBurst("Vase", Numbered("Obstacles/Vase/Particles/particle_vase_{0:00}.png", 3, 2, 0.2f, Pieces));
            var stone = CreateBurst("Stone", Numbered("Obstacles/Stone/Particles/particle_stone_{0:00}.png", 3, 2, 0.3f, Pieces));
            var chaliceDoor = CreateBurst("ChaliceDoor",
                Numbered("Obstacles/ChaliceBox/Particles/DoorParticles/chalice_box_door_particle_{0:00}.png", 10, 1, 0.45f, BigPieces));
            var chaliceBox = CreateBurst("ChaliceBox",
                Numbered("Obstacles/ChaliceBox/Particles/BaseParticles/chalice_box_particle_{0:00}.png", 5, 2, 0.35f, BigPieces));
            var tnt = CreateBurst("Tnt",
                new Part("SpecialItems/TNT/Particles/particle_tnt_01.png", 1, 1.4f, Flash),
                new Part("SpecialItems/TNT/Particles/particle_tnt_02.png", 10, 0.3f, Fire));
            var rocketPuff = CreateBurst("RocketPuff", new Part("SpecialItems/Rocket/Particles/particle_smoke.png", 4, 0.5f, Smoke));
            var rocketTrail = CreateTrail("RocketTrail",
                new Part("SpecialItems/Rocket/Particles/particle_smoke.png", 20, 0.45f, TrailSmoke),
                new Part("SpecialItems/Rocket/Particles/particle_star.png", 8, 0.35f, TrailSparks));

            AssignCubeEffects(cubeEffects);
            AssignItemEffects("Vase", hit: vase, breakEffect: vase);
            AssignItemEffects("Stone", hit: null, breakEffect: stone);
            AssignItemEffects("ChaliceBox", hit: chaliceDoor, breakEffect: chaliceBox);
            AssignItemEffects("Tnt", hit: null, breakEffect: tnt);
            AssignItemEffects("Rocket", hit: null, breakEffect: rocketPuff);
            AssetDatabase.SaveAssets();

            var boardView = Object.FindFirstObjectByType<BoardView>();
            var pool = boardView.gameObject.AddComponent<EffectPool>();
            SetReference(boardView, "effects", pool);
            SetReference(Object.FindFirstObjectByType<RocketEffect>(), "trailPrefab", rocketTrail);

            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Effects Setup", "Particle effects are ready.", "OK");
        }

        /// <summary>Parts for numbered textures (name_01, name_02, ...) that share a style.</summary>
        private static Part[] Numbered(string pathFormat, int textures, int countEach, float sizePer100Px, Style style)
        {
            var parts = new Part[textures];
            for (int i = 0; i < textures; i++)
                parts[i] = new Part(string.Format(pathFormat, i + 1), countEach, sizePer100Px, style);
            return parts;
        }

        /// <summary>A one-shot effect: the first part is the root system, the others are child systems played with it.</summary>
        private static ParticleSystem CreateBurst(string name, params Part[] parts) => CreateEffect(name, false, parts);

        /// <summary>A looping effect that emits while it moves (per world unit travelled), for the rocket halves.</summary>
        private static ParticleSystem CreateTrail(string name, params Part[] parts) => CreateEffect(name, true, parts);

        private static ParticleSystem CreateEffect(string name, bool trail, Part[] parts)
        {
            var root = new GameObject(name);
            for (int i = 0; i < parts.Length; i++)
            {
                var systemObject = i == 0 ? root : new GameObject($"{name}{i + 1}");
                systemObject.transform.SetParent(i == 0 ? null : root.transform, false);
                Configure(systemObject.AddComponent<ParticleSystem>(), parts[i], trail);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{EffectsFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab.GetComponent<ParticleSystem>();
        }

        private static void Configure(ParticleSystem system, Part part, bool trail)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var style = part.Style;
            var texture = LoadTexture(part.Texture);
            float size = texture.width / 100f * part.SizePer100Px;

            var main = system.main;
            main.playOnAwake = false;
            main.loop = trail;
            main.duration = trail ? 1f : 0.1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(style.Lifetime.x, style.Lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(style.Speed.x, style.Speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.8f, size * 1.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = style.Color;
            main.gravityModifier = style.Gravity;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            if (trail)
                emission.rateOverDistance = part.Count;
            else
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, part.Count) });

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = Mathf.Max(style.Radius, 0.001f);

            if (style.Spin > 0f)
            {
                var rotation = system.rotationOverLifetime;
                rotation.enabled = true;
                rotation.z = new ParticleSystem.MinMaxCurve(-style.Spin, style.Spin);
            }

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, style.Grows
                ? AnimationCurve.Linear(0f, 0.5f, 1f, 1.2f)
                : AnimationCurve.Linear(0f, 1f, 1f, 0.5f));

            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, style.FadeFrom), new GradientAlphaKey(0f, 1f) });
            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.material = MaterialFor(part.Texture, texture, style.Additive);
            renderer.sortingOrder = trail ? TrailSortingOrder : EffectSortingOrder;
            if (trail)
                renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        /// <summary>One material per texture and blend mode: additive for glows and sparks, alpha blended for pieces and smoke.</summary>
        private static Material MaterialFor(string texturePath, Texture2D texture, bool additive)
        {
            string key = texturePath + (additive ? "+add" : "");
            if (Materials.TryGetValue(key, out var material))
                return material;

            string shader = additive ? "Legacy Shaders/Particles/Additive" : "Legacy Shaders/Particles/Alpha Blended";
            material = new Material(Shader.Find(shader)) { mainTexture = texture };
            AssetDatabase.CreateAsset(material, $"{MaterialsFolder}/{texture.name}{(additive ? "_Additive" : "")}.mat");
            Materials.Add(key, material);
            return material;
        }

        private static void AssignCubeEffects(Dictionary<CubeColor, ParticleSystem> effects)
        {
            var cube = AssetDatabase.LoadAssetAtPath<GameObject>(ItemPrefabsFolder + "Cube.prefab").GetComponent<CubeView>();
            var serialized = new SerializedObject(cube);
            var entries = serialized.FindProperty("colorVisuals");
            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                var color = (CubeColor)entry.FindPropertyRelative("Color").enumValueIndex;
                entry.FindPropertyRelative("BreakEffect").objectReferenceValue = effects[color];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cube.gameObject);
        }

        private static void AssignItemEffects(string prefabName, ParticleSystem hit, ParticleSystem breakEffect)
        {
            var view = AssetDatabase.LoadAssetAtPath<GameObject>(ItemPrefabsFolder + prefabName + ".prefab").GetComponent<ItemView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("hitEffect").objectReferenceValue = hit;
            serialized.FindProperty("breakEffect").objectReferenceValue = breakEffect;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view.gameObject);
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static void SetReference(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Texture2D LoadTexture(string pathInArt)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtRoot + pathInArt);
            if (texture == null)
                throw new System.InvalidOperationException($"Texture not found: {ArtRoot}{pathInArt}");
            return texture;
        }
    }
}
