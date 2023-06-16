using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;

public class OcbModelMaterialChanger : IModApi
{

    // ####################################################################
    // ####################################################################

    public void InitMod(Mod mod)
    {
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        // mod.Ev
    }

    // ####################################################################
    // ####################################################################

    class ShaderConfig
    {
        public Dictionary<string, Texture> textures
            = new Dictionary<string, Texture>();
        public Dictionary<string, Color> colors
            = new Dictionary<string, Color>();
        public Dictionary<string, float> floats
            = new Dictionary<string, float>();
        public Shader shader = null;
    }

    class ZedConfig
    {
        public Dictionary<string, ShaderConfig> Materials
            = new Dictionary<string, ShaderConfig>();
        public Dictionary<string, ShaderConfig> Shaders
            = new Dictionary<string, ShaderConfig>();
        public bool debug = false;
    }

    static Dictionary<string, ZedConfig> ZedConfigs
        = new Dictionary<string, ZedConfig>();

    // ####################################################################
    // ####################################################################

    static void ParseConfig(XElement xml, Dictionary<string, ShaderConfig> cfgs)
    {
        string id = xml.HasAttribute("name") ? xml.GetAttribute("name") :
            throw new Exception("Attribute 'name' missing on property in entity_class");
        if (!cfgs.TryGetValue(id, out ShaderConfig cfg))
            cfg = cfgs[id] = new ShaderConfig();
        foreach (XElement node in xml.Elements())
        {
            if (!node.HasAttribute("value")) throw new
                Exception($"{xml.Name} is missing value");
            string value = node.Attribute("value").Value;
            if (node.Name == "shader")
            {
                var path = DataLoader.ParseDataPathIdentifier(value);
                AssetBundleManager.Instance.LoadAssetBundle(path.BundlePath);
                var shader = cfg.shader = AssetBundleManager.Instance
                    .Get<Shader>(path.BundlePath, path.AssetName);
                if (shader == null) Log.Warning("Could not load {0}", value);
            }
            else
            {
                if (!node.HasAttribute("name")) throw new
                    Exception($"{xml.Name} is missing name");
                string name = node.Attribute("name").Value;
                if (node.Name == "texture")
                {
                    var path = DataLoader.ParseDataPathIdentifier(value);
                    AssetBundleManager.Instance.LoadAssetBundle(path.BundlePath);
                    var texture = cfg.textures[name] = AssetBundleManager.Instance
                        .Get<Texture>(path.BundlePath, path.AssetName);
                    if (texture == null) Log.Warning("Could not load {0}", value);
                }
                else if (node.Name == "color")
                {
                    cfg.colors[name] = StringParsers.ParseColor(value);
                }
                else if (node.Name == "float")
                {
                    cfg.floats[name] = float.Parse(value);
                }
            }
        }
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(EntityClassesFromXml), "LoadEntityClasses")]
    public class EntityClassesFromXmlLoadEntityClassesPatch
    {
        static void Postfix(XmlFile _xmlFile)
        {
            ZedConfigs.Clear();
            XElement root = _xmlFile.XmlDoc.Root;
            foreach (XElement el in root.Elements())
            {
                if (el.Name != "entity_class") continue;
                string name = el.HasAttribute("name") ? el.GetAttribute("name") :
                    throw new Exception("Attribute 'name' missing on property in entity_class");
                bool debug = el.HasAttribute("debug") && bool.Parse(el.GetAttribute("debug"));
                foreach (XElement node in el.Elements())
                {
                    if (node.Name == "material")
                    {
                        if (!ZedConfigs.TryGetValue(name, out var cfg))
                            cfg = ZedConfigs[name] = new ZedConfig();
                        ParseConfig(node, cfg.Materials);
                        cfg.debug = debug;
                    }
                    else if (node.Name == "shader")
                    {
                        if (!ZedConfigs.TryGetValue(name, out var cfg))
                            cfg = ZedConfigs[name] = new ZedConfig();
                        ParseConfig(node, cfg.Shaders);
                        cfg.debug = debug;
                    }
                }
            }
        }
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(EModelBase), "createModel")]
    public class EModelBaseCreateModelPatch
    {
        static void Postfix(EModelBase __instance, EntityClass _ec)
        {
            if (!(ZedConfigs.TryGetValue(_ec.entityClassName, out ZedConfig zed))) return;
            SkinnedMeshRenderer[] renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
            int max = 0; foreach (var r in renderers) max = Math.Max(max, r.materials.Length);
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    string mat = material.name;
                    string shn = material.shader.name;
                    if (mat.EndsWith(" (Instance)")) mat =
                        mat.Substring(0, mat.Length - 11);
                    if (shn.EndsWith(" (Instance)")) shn =
                        shn.Substring(0, shn.Length - 11);
                    if (zed.Shaders.TryGetValue(shn, out var cfg))
                        ApplyMaterialConfig(material, cfg);
                    if (zed.Materials.TryGetValue(mat, out cfg))
                        ApplyMaterialConfig(material, cfg);
                }
            }
            // Call debug after changes are applied (e.g. new shader)
            if (zed.debug == true) DebugTransform(__instance, _ec);
        }

    }

    // ####################################################################
    // ####################################################################

    private static void ApplyMaterialConfig(Material material, ShaderConfig cfg)
    {
        if (cfg.shader != null) material.shader = cfg.shader;
        foreach (var texture in cfg.textures) material
            .SetTexture(texture.Key, texture.Value);
        foreach (var color in cfg.colors) material
            .SetColor(color.Key, color.Value);
        foreach (var value in cfg.floats) material
            .SetFloat(value.Key, value.Value);
    }

    // ####################################################################
    // ####################################################################

    private static void DebugTransform(EModelBase instance, EntityClass ec)
    {
        Log.Out("===============================================");
        Log.Out("Debug Transform of {0}", ec.entityClassName);
        DebugRecursive(instance.transform, "");
        Log.Out("===============================================");
    }

    private static void DebugRecursive(Transform node, string ind)
    {
        for (int i = 0; i < node.childCount; i++)
        {
            Transform child = node.GetChild(i);
            Log.Out(ind + "[{0}]", child.name);
            foreach (var renderer in child.GetComponents<Renderer>())
                DebugRenderer(renderer, ind);
            DebugRecursive(child, ind + "  ");
        }
    }

    private static void DebugRenderer(Renderer renderer, string ind)
    {
        Log.Out(ind + "{{renderer}} {0}", renderer);
        for (int m = 0; m < renderer.materials.Length; m++)
        {
            Material material = renderer.materials[m];
            Log.Out(ind + "{{material{0}}} {1}", m, material.name);
            Log.Out(ind + " shader {0}", material.shader.name);
            for (int i = 0; i < material.shader.GetPropertyCount(); i++)
                Log.Out(ind + " <{0}> {1} {2}", i,
                    material.shader.GetPropertyType(i),
                    material.shader.GetPropertyName(i));
        }
    }

    // ####################################################################
    // ####################################################################

}
