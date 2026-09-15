EXTERNAL SetFlag(flagName)
EXTERNAL HasFlag(flagName)
EXTERNAL MoveCharacter(id, direction, tiles)

# name: 村人
あっちに何か落ちてる。ちょっと見てくる。

~ MoveCharacter("Test", "Right", 3)

# name: 村人
...

# name: 村人
……なんだ、ただの石か。

~ SetFlag("VillageEventFinished")

-> END