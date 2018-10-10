using System.Reflection;
using JetBrains.Annotations;
using Modding;
using TMPro;

namespace QoL
{
    [UsedImplicitly]
    public class FastText : Mod, ITogglableMod
    {
        private static readonly FieldInfo TEXT_MESH = typeof(DialogueBox).GetField("textMesh", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void Initialize()
        {
            On.DialogueBox.ShowNextChar += OnNextChar;
        }

        public void Unload()
        {
            On.DialogueBox.ShowNextChar -= OnNextChar;
        }

        private static void OnNextChar(On.DialogueBox.orig_ShowNextChar orig, DialogueBox self)
        {
            var text = (TextMeshPro) TEXT_MESH.GetValue(self);
            text.maxVisibleCharacters = text.textInfo.pageInfo[self.currentPage - 1].lastCharacterIndex + 1;
        }

    }
}
