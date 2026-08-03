EXTERNAL SetFlag(flagName)
EXTERNAL HasFlag(flagName)


{ HasFlag("MetVillageChief"):
	#name: 村長
	これは村長と会話した後の文章です。
- else:
	~ SetFlag("MetVillageChief")
	#name: 村長
	まだ会話していません。
}
-> END