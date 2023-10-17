# Character Customizer v1.1 for Unity

If you run into any issues feel free to reach out at lindborgdev@gmail.com or on Discord (https://discord.com/invite/vMVE2kuwzV) and I'll see what I can do

**General overview**

To get an idea of how Character Customizer works, open CC_Demo_Scene and, if prompted, import Text Mesh Pro essentials.

The general layout of Character Customizer looks like this:

- Character Parent
  - Character 1
    - CharacterCustomization script
  - Character 2
    - CharacterCustomization script
  - Character 3
    - CharacterCustomization script
- CameraController script
  - Camera
- CC_UI_Manager script
- Character 1 UI
  - CC_UI_Util script
- Character 2 UI
  - CC_UI_Util script
- Character 3 UI
  - CC_UI_Util script

In the demo scene each character has a respective UI assigned in their CharacterCustomization script. Without a UI assigned, the character will still load from save file, customization still works but there will be no interface to assist with it. When the character's Start() runs it calls the function Initialize in CC_UI_Util, which in turn runs the ICustomizerUI interface function InitializeUIElement for every child object that implements the interface. This is how the UI elements (sliders, pickers etc) are linked to the character's CharacterCustomization script, and how they get updated from the save data.

CC_UI_Manager is a simple singleton used for switching between the characters and their UIs and playing audio clips.

CameraController is a script for controlling the camera and has some additional functionality for context sensitive mouse cursors.

The CharacterCustomization script contains almost all the functionality for changing properties and blendshapes, saving and loading and managing clothes and hair. The UI elements contain little functionality by themselves and just serve as a graphical interface for the main script.

Much of the customization in Character Customizer is designed around the class _CC_Property_. This struct is used to store blendshapes, texture properties, float properties and color properties. Certain variables in a CC_Property may be superfluous depending on the use case.

- _propertyName_ is the blendshape name or the material property name
- _stringValue_ stores a string, which is usually a reference to a resource, or an HTML color
- _floatValue_ stores a float, for example a scalar value or a blendshape value
- _materialIndex_ is the material index the property should be set on, the default -1 means it should be set on all materials
- _meshTag_ lets you set the property on a mesh renderer gameObject with a specific tag, default "" means it's set on all meshes

The UI element scripts generally have one or more CC_Property to determine what properties they affect.

**Characters**

The characters have four LODs at 73568, 21794, 10676 and 2674 triangles. They have FACS blendshapes for facial animation, viseme blendshapes (OVR standard) for lipsync and a bunch of blendshapes for changing the shape of the face.

They have custom shader graphs with customizable properties (skin texture, eye color and so on). You can add more properties, optionally using one of the included material functions (MF_Detail_Multiply which multiplies the texture with the skin texture, or MF_Detail_Tintable which lerps it in using a black and white mask). A note on texture properties - if you add a T2 property that you want to implement in the menu you need to make sure that your textures are located in a folder called "Resources" somewhere in the package directory, otherwise Unity won't be able to load them from name.

**UI elements**

The UIs (UI_Human and UI_Dummy) work through simple drag and drop. The UI prefabs are located in _UI/Prefabs/Style1_ and the scripts are in _UI/Scripts_. The main scripts are the following:

_Option_Slider_ has two modes, Blendshape and Scalar. It sets the blendshape value or scalar property with the assigned name. You can set the slider range on the slider itself to set a value other than 0 to 1, for example some sliders are remapped to the -1 to 1 range.

_Option_Proportional_Sliders_ takes a list of objects (blendshape names) and creates one slider for each with a capped sum of 1. If changing the value of a slider makes the total sum exceed 1 it subtracts the excess proportionally from the other sliders.

_Option_Picker_ has four modes - Blendshape, Texture, Hair and Apparel. It takes a list of objects (blendshape names/texture resource names).

- Blendshape mode sets the value to 1 for the current index and 0 for the other indices.
- Texture mode loads the resource from the current index and sets it as a property.
- In hair and apparel mode the Objects list is empty. Tnstead they take an integer "Slot" and fetches options from the Apparel or Hair scriptable object on the active character's customization script. The slot is the index of the scriptable object (more on this below).

_Option_Color_Picker_ has four sliders, three for RGB and one for Opacity, which is not to be confused with the RGBA kind of opacity but rather it sets a scalar parameter which can be used to control the strength of the effect in the shader. There are prefabs for color pickers with (e.g. skin tint) and without (eye color) an Opacity slider.

**Hair/Apparel scriptable objects**

Clothes and hair are instantiated from data fetched from scriptable objects, scrObj_Apparel and scrObj_Hair respectively. These scriptable objects are assigned on CharacterCustomization under Apparel Tables and Hair Tables and there should be one table per category you want (categories are "Hair", "Beard", "Upper Body", "Footwear" etc). The two scriptable objects are similar but have some key differences.

scrObj_Apparel looks like this:

- _Mesh_ is the prefab containing the mesh and the skeleton and what not
- _Name_ is the name that gets added to the save file and is used to identify the item later
- _Add Copy Pose Script_ is useful if the skeletal hierarchy of the mesh is different from the character it is attached to. In standard cases the skeletons should be identical, meaning they can be merged to save performance, but in some cases you will want to have extra bones added for physics or some other purpose, and bone transforms will have to be manually copied
- _Mask_ is an optional texture that I use to simplify my character workflow by being able to mask off a certain part of the character's body based on the clothing that's equipped
- _Foot Offset_ and _Affect Foot Offset_ pertain to the FootOffset script, which allows apparel to affect the character's skeleton based on the equipped footwear. _Height Offset_ is an offset to the root bone, _Foot Rotation_ and _Ball Rotation_ apply a rotation offset to the feet and ball bones
- _Icon_ is an optional icon used for the Grid_Icon buttons
- The scriptable object itself has a _Mask Property_ which is the name of the property on the skin shader

scrObj_Hair works much the same way with some differences:

- _Shadow Map_ is (optionally) used to apply a shadow map on the character's head material to help blend the hair and the scalp
- _Shadow Map Property_ is the texture property on the skin shader
- _Tint Property_ is the color property on the skin shader

If you want to add more clothes/hair, you just need to add the item to the appropriate object in _Characters/Human/Apparel/Hair/Scriptable_Objects_.

For more details on how that works, check out setHair and setApparel in CharacterCustomization.

**Character presets**

The characters have one final scriptable object which is scrObjPresets. This is a list of CC_CharacterData which can be used to store preset characters. One quick and easy way of adding a preset is by customizing a character in game, selecting the character in the hierarchy and copying the Stored Character Data from the CharacterCustomization script. Then you can simply paste it into the preset scriptable object as a new element and store it for later. Give it a unique name and you can get it with Presets.Presets.Find(t => t.CharacterName == "unique name") or something like that. The first element in the presets object is used as the default character preset in the CharacterCustomization script when no character has been saved to file.

**Compatibility**

Character Customizer is by default compatible with HDRP but works with all rendering pipelines after some tweaking of the shaders and materials. For Built-In, you need to install Shader Graph from the package manager. For both URP and Built-In, open up all the shader graphs in the package (search for shader) and add your target in the Graph Inspector (top right). Some materials in the project also need to be updated to use the rendering pipelines standard shaders, for example M_Eyelashes, M_Eye_Occlusion, M_Glass, M_Background.
