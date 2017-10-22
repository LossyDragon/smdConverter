####Source to Goldsrc SMD Converter####

Fork: https://github.com/LossyDragon/smdConverter
Original: https://github.com/Petethegoat/smdConverter

This is designed as a substitute for Milkshape in the process of bringing a model from Blender (with the SMD Tools plugin) into Half-Life 1 and Goldsrc based mods. It should also be useful for Source .smds exported from other tools, and is by no means Blender specific.

It converts Source style bone assignment to Goldsrc compatible style. I've successfully tested it with animations and head controllers. If your model doesn't have any bones, then you don't need to use this! You should be able to compile the Source .smd directly and use that without issues.

Features: Blender and MESA support, TGA to BMP replacement, optional underscore to space in node names, decimal placement in all 0 vertex, and e-notation conversion

#####Usage:#####

Just drag a Source .smd onto S2GSMDC2.exe, and go through the prompts. You can drag multiple files onto it at once. You may also use it from the command line.

#####Changelog:#####

*	v1.0 October 9, 2017
	Forked from Petethegoat. Added MESA support, file extension coversion (TGA to BMP), added robustness to fix quirks with SMD formatting so no corruption to the vertex's happen. 

#####Known issues: (from the orignal author)#####

* It doesn't handle boneweights particularly gracefully right now so try and avoid setting them to anything other than 0 or 1.
Goldsrc doesn't have any support for skinning a vertex to more than one bone anyway, as far as I know.

* That's all! If you encounter any issues, send me an email at petethegoat@gmail.com, or try querying Petethegoat on irc.rizon.net, or irc.gamesurge.net.
You can also file an issue report at:
https://github.com/Petethegoat/smdConverter
