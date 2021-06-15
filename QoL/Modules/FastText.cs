using JetBrains.Annotations;
using TMPro;
using Vasi;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class FastText : FauxMod
    {
        public override void Initialize()
        {
            On.DialogueBox.ShowNextChar += OnNextChar;
        }

        public override void Unload()
        {
            On.DialogueBox.ShowNextChar -= OnNextChar;
        }

        private static void OnNextChar(On.DialogueBox.orig_ShowNextChar orig, DialogueBox self)
        {
            TextMeshPro text = Mirror.GetField<DialogueBox, TextMeshPro>(self, "textMesh");
            
            text.maxVisibleCharacters = text.textInfo.pageInfo[self.currentPage - 1].lastCharacterIndex + 1;
        }
    }
}