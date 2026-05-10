CG resources fallback directory for DialogueSystem.

Place CG sprites here so they can be referenced in dialogue scripts via:
    [bg:Song_talk_1]

The DialogueSystem will first look in BackgroundConfig, then fall back to:
    Resources/CG/<name>
    Resources/UI/Dialogue/Backgrounds/<name>

To use the existing CG assets from Art/Characters/cg/:
1. In Unity, copy or move the Sprite assets (e.g. Song_talk_1.png ~ Song_talk_4.png)
   into this folder (Assets/Resources/CG/).
2. Reference them in Dialogue2.txt with [bg:Song_talk_1] etc.
