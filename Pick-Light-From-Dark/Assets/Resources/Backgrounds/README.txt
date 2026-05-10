Background resources directory for DialogueSystem.

Place background sprites here so they can be referenced in dialogue scripts via:
    [bg:the spread quilt]
    [bg:quilt aside]

Search priority (first match wins):
1. Resources/CG/<name>
2. Resources/Backgrounds/<name>
3. Resources/UI/Dialogue/Backgrounds/<name>
4. BackgroundConfig ScriptableObject

To use existing scene assets from Art/Scene/:
1. In Unity, copy or move the Sprite assets into this folder.
2. Reference them in TXT with [bg:filename] (no .png extension needed).
