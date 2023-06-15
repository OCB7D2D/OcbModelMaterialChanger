# OCB Core Mod for Custom Zombie Textures  - 7 Days to Die (A21) Addon

This mod allows other mods to define new zombie entities from existing
ones and replacing/changing any property of the shader materials. Allows
you to effectively swap out textures with your custom ones.

## Syntax

This mod will parse a few additional tags for the entity class. Custom
configurations can be applied by shader or material name.

```xml
<configs>
  <append xpath="/entity_classes">
    <entity_class name="ZARLENE_NEW" extends="zombieArlene" debug="false">
      <shader name="Game/Character"><!-- or <material name="HD_Arlene"> -->
        <texture name="_Albedo" value="#@modfolder:Resources/Arlene.unity3d?HD_Arlene"/>
        <texture name="_Normal" value="#@modfolder:Resources/Arlene.unity3d?HD_Arlene_n"/>
        <texture name="_RMOE" value="#@modfolder:Resources/Arlene.unity3d?HD_Arlene_RMOE"/>
        <color name="_EmissiveColor" value="0,0,0,0"/>
        <float name="_Irradiated" value="1"/>
      </shader>
    </entity_class>
  </append>
</configs>
```

#### How to find out what can be overwritten

This mod includes a small debug utility that may help modders to find what shader/material
properties can be changed. It will also display the shader and material names you'll need:

To setup the debug mode, you will need to add a `debug="true"` attribute to it:
```xml
<entity_class name="ZARLENE_NEW" extends="zombieArlene" debug="true" />
```

After that you will need to spawn your new zombie in-game and you should get some debug output:

```
Debug Transform of ZARLENE_NEW
[FeralEye]
{renderer} FeralEye (UnityEngine.SkinnedMeshRenderer)
{material0} HD_Arlene (Instance)
 shader Game/Character
 <0> Float _Irradiated
 <1> Texture _Albedo
 <2> Texture _Normal
 <3> Texture _RMOE
 <4> Color _EmissiveColor
 <5> Texture _texcoord
 <6> Float __dirty
[hairLOD0]
{renderer} hairLOD0 (UnityEngine.SkinnedMeshRenderer)
{material0} HD_ArleneHair (Instance)
 shader Game/Autodesk
 <0> Color _Color
 <1> Texture _MainTex
 <2> Range _Cutoff
 <3> Range _Glossiness
 <4> Texture _SpecGlossMap
 <5> Range _Metallic
 <6> Texture _MetallicGlossMap
 <7> Float _SpecularHighlights
 <8> Float _GlossyReflections
 <9> Float _BumpScale
 <10> Texture _BumpMap
 <11> Range _Parallax
 <12> Texture _ParallaxMap
 <13> Range _OcclusionStrength
 <14> Texture _OcclusionMap
 <15> Color _EmissionColor
 <16> Texture _EmissionMap
 <17> Texture _DetailMask
 <18> Texture _DetailAlbedoMap
 <19> Float _DetailNormalMapScale
 <20> Texture _DetailNormalMap
 <21> Float _UVSec
 <22> Float _Mode
 <23> Float _SrcBlend
 <24> Float _DstBlend
 <25> Float _ZWrite
[LOD0]
{renderer} LOD0 (UnityEngine.SkinnedMeshRenderer)
{material0} HD_Arlene (Instance)
 shader Game/Character
 <0> Float _Irradiated
 <1> Texture _Albedo
 <2> Texture _Normal
 <3> Texture _RMOE
 <4> Color _EmissiveColor
 <5> Texture _texcoord
 <6> Float __dirty
...
```

Note: that not all properties are meant to be set this way. I also don't know
what all of these do, and some textures have weird channel usage, so you'll
need to find out how to use this to the fullest for yourself!

## Download and Install

Simply [download here from GitHub][1] and put into your A20 Mods folder:

- https://github.com/OCB7D2D/OcbModelMaterialChanger/archive/master.zip (master branch)

## Changelog

### Version 0.1.0

- Initial test version

## Compatibility

I've developed and tested this Mod against version a21.0(b317).

[1]: https://github.com/OCB7D2D/OcbModelMaterialChanger/releases
[2]: https://github.com/OCB7D2D/OcbModelMaterialChanger/actions/workflows/ci.yml
[3]: https://github.com/OCB7D2D/OcbModelMaterialChanger/actions/workflows/ci.yml/badge.svg
